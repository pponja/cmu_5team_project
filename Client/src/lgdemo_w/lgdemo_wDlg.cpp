
// lgdemo_wDlg.cpp: 구현 파일
//

#include "pch.h"
#include "framework.h"
#include "lgdemo_w.h"
#include "lgdemo_wDlg.h"
#include "afxdialogex.h"

#include <Windows.h>
#include "NetworkTCP.h"
#include <map>
#include <iostream>
#include <stdlib.h>
#include <tchar.h> 
#include "opencv2/opencv.hpp"
#include "support/timing.h"
#include "motiondetector.h"
#include "alpr.h"
#include "DeviceEnumerator.h"
#include <atlstr.h>
#include <strsafe.h>
#include <cderr.h>

//migration
#include "NetworkTCP.h"
#include <windows.h>
#include <map>
#include <iostream>
#include <stdlib.h>
#include <tchar.h> 
#include "opencv2/opencv.hpp"
#include "support/timing.h"
#include "motiondetector.h"
#include "alpr.h"
#include "DeviceEnumerator.h"
#include <atlstr.h>
#include <strsafe.h>





#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define BUFFER_SIZE		1024 // 1K


using namespace alpr;
using namespace std;
using namespace cv;


enum class Mode { mNone, mLive_Video, mPlayback_Video, mImage_File };
enum class VideoResolution { rNone, r640X480, r1280X720 };
enum class VideoSaveMode { vNone, vNoSave, vSave, vSaveWithNoALPR };
enum class ResponseMode { ReadingHeader, ReadingMsg };

ResponseMode GetResponseMode = ResponseMode::ReadingHeader;
short RespHdrNumBytes;
char ResponseBuffer[2048];
unsigned int BytesInResponseBuffer = 0;
ssize_t BytesNeeded = sizeof(RespHdrNumBytes);

//IPC
HANDLE m_hPipe = nullptr;


//ADD OpenCV
Mode mode;
VideoSaveMode videosavemode;
VideoResolution vres;

char filename[MAX_PATH];

char text[1024] = "";
int frameno = 0;
VideoCapture cap;
VideoWriter outputVideo;
int deviceID = -1;
int apiID = cv::CAP_ANY;      // 0 = autodetect default API
//
bool measureProcessingTime = false;
std::string templatePattern;
MotionDetector motiondetector;
bool do_motiondetection = false;

bool _qpcInited = false;
double PCFreq = 0.0;
__int64 CounterStart = 0;
double _avgdur = 0;
double _fpsstart = 0;
double _avgfps = 0;
double _fps1sec = 0;
TTcpConnectedPort* TcpConnectedPort;

#define NUMBEROFPREVIOUSPLATES 10
char LastPlates[NUMBEROFPREVIOUSPLATES][64] = { "","","","","" };
unsigned int CurrentPlate = 0;

static VideoSaveMode GetVideoSaveMode(void);
static VideoResolution GetVideoResolution(void);
static Mode GetVideoMode(void);
static int GetVideoDevice(void);
static bool GetFileName(Mode mode, char filename[MAX_PATH]);
static bool detectandshow(Alpr* alpr, cv::Mat frame, std::string region, bool writeJson);
static void InitCounter();
static double CLOCK();
static bool getconchar(KEY_EVENT_RECORD& krec);
static double avgdur(double newdur);
static double avgfps();
static void GetResponses(LPVOID param);
/***********************************************************************************/
/* Main                                                                            */
/***********************************************************************************/




// 응용 프로그램 정보에 사용되는 CAboutDlg 대화 상자입니다.

class CAboutDlg : public CDialogEx
{
public:
	CAboutDlg();

// 대화 상자 데이터입니다.
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_ABOUTBOX };
#endif

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

// 구현입니다.
protected:
	DECLARE_MESSAGE_MAP()
public:
   
};

CAboutDlg::CAboutDlg() : CDialogEx(IDD_ABOUTBOX)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialogEx)
END_MESSAGE_MAP()


// ClgdemowDlg 대화 상자



ClgdemowDlg::ClgdemowDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_LGDEMO_W_DIALOG, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

std::wstring ExePath() {
    TCHAR buffer[MAX_PATH] = { 0 };
    GetModuleFileName(NULL, buffer, MAX_PATH);
    std::wstring::size_type pos = std::wstring(buffer).find_last_of(L"\\/");
    return std::wstring(buffer).substr(0, pos);
}
HANDLE createdNamedPipe()
{
    HANDLE hPipe = nullptr;
    CString strPipeName;
    strPipeName.Format(_T("\\\\%s\\pipe\\%s"),
        _T("."),
        _T("IPC_DATA_CHANNEL")	
    );


    while (TRUE)
    {
        hPipe = CreateFile(
            strPipeName,			
            GENERIC_READ |			
            GENERIC_WRITE,
            0,						
            NULL,					
            OPEN_EXISTING,			
            0,						
            NULL);					

       
        if (hPipe != INVALID_HANDLE_VALUE)
            break;

        if (
            GetLastError() != ERROR_PIPE_BUSY
            ||
          
            !WaitNamedPipe(strPipeName, 5000))
        {
            _tprintf(_T("Unable to open named pipe %s w/err 0x%08lx\n"),
                strPipeName, GetLastError());

        }
    }
    _tprintf(_T("The named pipe, %s, is connected.\n"), strPipeName);

    return hPipe;

}
bool writeMessage(HANDLE hPipe, CString temp)
{
       DWORD dwMode = PIPE_READMODE_MESSAGE;
    BOOL bResult = SetNamedPipeHandleState(hPipe, &dwMode, NULL, NULL);
    if (!bResult)
    {
        _tprintf(_T("SetNamedPipeHandleState failed w/err 0x%08lx\n"),
            GetLastError());
        return 1;
    }
       
    TCHAR chRequest[BUFFER_SIZE];	
    DWORD cbBytesWritten, cbRequestBytes;
    TCHAR chReply[BUFFER_SIZE];		
    DWORD cbBytesRead, cbReplyBytes;

    // Send one message to the pipe.
    StringCchCopy(chRequest, BUFFER_SIZE, temp);
    cbRequestBytes = sizeof(TCHAR) * (lstrlen(chRequest) + 1);

    bResult = WriteFile(			
        hPipe,						
        chRequest,					
        cbRequestBytes,				
        &cbBytesWritten,		
        NULL);						

    if (!bResult || cbRequestBytes != cbBytesWritten)
    {
        _tprintf(_T("WriteFile failed w/err 0x%08lx\n"), GetLastError());
        return 1;
    }

    _tprintf(_T("Sends %ld bytes; Message: \"%s\"\n"),
        cbBytesWritten, chRequest);

}
void executeProgram(CString fileName, bool bWait = FALSE)
{
    TCHAR programpath[_MAX_PATH];
    wstring temp = ExePath();
   const wchar_t* wcs = temp.c_str();

    SHELLEXECUTEINFO ShExecInfo = { 0 };
    ShExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
    ShExecInfo.fMask = SEE_MASK_NOCLOSEPROCESS;
    ShExecInfo.hwnd = NULL;
    ShExecInfo.lpVerb = NULL;
    ShExecInfo.lpFile = fileName;
    ShExecInfo.lpParameters = L"";
    ShExecInfo.lpDirectory = wcs;
    ShExecInfo.nShow = SW_SHOW;
    ShExecInfo.hInstApp = NULL;
    ShellExecuteEx(&ShExecInfo);
    if (bWait)
    {
        WaitForSingleObject(ShExecInfo.hProcess, 2000);
        CloseHandle(ShExecInfo.hProcess);
    }
}
void ClgdemowDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_PIC, m_PIC);
	DDX_Control(pDX, IDC_BUTTON3, Btn_playback);
	DDX_Control(pDX, IDC_BUTTON2, Btn_LiveCam);

}

BEGIN_MESSAGE_MAP(ClgdemowDlg, CDialogEx)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_DESTROY()
	ON_WM_TIMER()
	ON_WM_CREATE()
    ON_WM_CLOSE()
    ON_BN_CLICKED(IDC_BUTTON3, &ClgdemowDlg::OnBnClickedButton_playback)
    ON_BN_CLICKED(IDC_BUTTON2, &ClgdemowDlg::OnBnClickedButton_LiveCam)
END_MESSAGE_MAP()


// ClgdemowDlg 메시지 처리기
BOOL ClgdemowDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// 시스템 메뉴에 "정보..." 메뉴 항목을 추가합니다.

	// IDM_ABOUTBOX는 시스템 명령 범위에 있어야 합니다.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != nullptr)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// 이 대화 상자의 아이콘을 설정합니다.  응용 프로그램의 주 창이 대화 상자가 아닐 경우에는
	//  프레임워크가 이 작업을 자동으로 수행합니다.
	SetIcon(m_hIcon, TRUE);			// 큰 아이콘을 설정합니다.
	SetIcon(m_hIcon, FALSE);		// 작은 아이콘을 설정합니다.

	// TODO: 여기에 추가 초기화 작업을 추가합니다.

	return TRUE;  // 포커스를 컨트롤에 설정하지 않으면 TRUE를 반환합니다.
}

void ClgdemowDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialogEx::OnSysCommand(nID, lParam);
	}
}

// 대화 상자에 최소화 단추를 추가할 경우 아이콘을 그리려면
//  아래 코드가 필요합니다.  문서/뷰 모델을 사용하는 MFC 애플리케이션의 경우에는
//  프레임워크에서 이 작업을 자동으로 수행합니다.

void ClgdemowDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // 그리기를 위한 디바이스 컨텍스트입니다.

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// 클라이언트 사각형에서 아이콘을 가운데에 맞춥니다.
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// 아이콘을 그립니다.
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

// 사용자가 최소화된 창을 끄는 동안에 커서가 표시되도록 시스템에서
//  이 함수를 호출합니다.
HCURSOR ClgdemowDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}



void ClgdemowDlg::OnDestroy()
{
	CDialogEx::OnDestroy();

	// TODO: 여기에 메시지 처리기 코드를 추가합니다.
}


void ClgdemowDlg::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: 여기에 메시지 처리기 코드를 추가 및/또는 기본값을 호출합니다.
    
	CDialogEx::OnTimer(nIDEvent);
}


int ClgdemowDlg::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CDialogEx::OnCreate(lpCreateStruct) == -1)
		return -1;
    //executeProgram(L"collectData.exe",TRUE);
    m_hPipe = createdNamedPipe();
   
    /*executeProgram(L"server.exe",TRUE);
   
    
    
    if ((TcpConnectedPort = OpenTcpConnection("127.0.0.1", "2222")) == NULL)
    {
        std::cout << "Connection Failed" << std::endl;
        return(0);
    }
    else std::cout << "Connected" << std::endl;*/

   
	return 0;
}

static const wchar_t* GetWC(const char* c)
{
    size_t cn;
    const size_t cSize = strlen(c) + 1;
    wchar_t* wc = new wchar_t[cSize];
    mbstowcs_s(&cn, wc, cSize, c, cSize);
    return wc;
}


/***********************************************************************************/
/* detectandshow                                                                   */
/***********************************************************************************/
static bool detectandshow(Alpr* alpr, cv::Mat frame, std::string region, bool writeJson)
{

    timespec startTime;
    getTimeMonotonic(&startTime);
    unsigned short SendPlateStringLength;
    ssize_t result;

    std::vector<AlprRegionOfInterest> regionsOfInterest;
    if (do_motiondetection)
    {
        cv::Rect rectan = motiondetector.MotionDetect(&frame);
        if (rectan.width > 0) regionsOfInterest.push_back(AlprRegionOfInterest(rectan.x, rectan.y, rectan.width, rectan.height));
    }
    else regionsOfInterest.push_back(AlprRegionOfInterest(0, 0, frame.cols, frame.rows));
    AlprResults results;
    if (regionsOfInterest.size() > 0) results = alpr->recognize(frame.data, (int)frame.elemSize(), frame.cols, frame.rows, regionsOfInterest);

    timespec endTime;
    getTimeMonotonic(&endTime);
    double totalProcessingTime = diffclock(startTime, endTime);
    if (measureProcessingTime)
        std::cout << "Total Time to process image: " << totalProcessingTime << "ms." << std::endl;


    if (writeJson)
    {
        std::cout << alpr->toJson(results) << std::endl;
    }
    else
    {
        for (int i = 0; i < results.plates.size(); i++)
        {
            char textbuffer[1024];
            std::vector<cv::Point2f> pointset;
            for (int z = 0; z < 4; z++)
                pointset.push_back(Point2i(results.plates[i].plate_points[z].x, results.plates[i].plate_points[z].y));
            cv::Rect rect = cv::boundingRect(pointset);
            cv::rectangle(frame, rect, cv::Scalar(0, 255, 0), 2);
            sprintf_s(textbuffer, "%s - %.2f", results.plates[i].bestPlate.characters.c_str(), results.plates[i].bestPlate.overall_confidence);

            cv::putText(frame, textbuffer,
                cv::Point(rect.x, rect.y - 5), //top-left position
                FONT_HERSHEY_COMPLEX_SMALL, 1,
                Scalar(0, 255, 0), 0, LINE_AA, false);


            if (m_hPipe)
            {
                bool found = false;
                for (int x = 0; x < NUMBEROFPREVIOUSPLATES; x++)
                {
                    if (strcmp(results.plates[i].bestPlate.characters.c_str(), LastPlates[x]) == 0)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    unsigned short SendMsgHdr;
                    SendPlateStringLength = (unsigned short)strlen(results.plates[i].bestPlate.characters.c_str()) + 1;
                    SendMsgHdr = htons(SendPlateStringLength);
                    printf("sent ->%s\n", results.plates[i].bestPlate.characters.c_str());
                    writeMessage(m_hPipe, GetWC(results.plates[i].bestPlate.characters.c_str()));
                }
            }
            strcpy_s(LastPlates[CurrentPlate], results.plates[i].bestPlate.characters.c_str());
            CurrentPlate = (CurrentPlate + 1) % NUMBEROFPREVIOUSPLATES;
#if 0
            std::cout << "plate" << i << ": " << results.plates[i].topNPlates.size() << " results";
            if (measureProcessingTime)
                std::cout << " -- Processing Time = " << results.plates[i].processing_time_ms << "ms.";
            std::cout << std::endl;

            if (results.plates[i].regionConfidence > 0)
                std::cout << "State ID: " << results.plates[i].region << " (" << results.plates[i].regionConfidence << "% confidence)" << std::endl;

            for (int k = 0; k < results.plates[i].topNPlates.size(); k++)
            {
                // Replace the multiline newline character with a dash
                std::string no_newline = results.plates[i].topNPlates[k].characters;
                std::replace(no_newline.begin(), no_newline.end(), '\n', '-');

                std::cout << "    - " << no_newline << "\t confidence: " << results.plates[i].topNPlates[k].overall_confidence;
                if (templatePattern.size() > 0 || results.plates[i].regionConfidence > 0)
                    std::cout << "\t pattern_match: " << results.plates[i].topNPlates[k].matches_template;

                std::cout << std::endl;
            }
#endif
        }
    }
    return results.plates.size() > 0;
}
/***********************************************************************************/
/* End detectandshow                                                               */
/***********************************************************************************/
/***********************************************************************************/
/* InitCounter                                                                     */
/***********************************************************************************/
static void InitCounter()
{
    LARGE_INTEGER li;
    if (!QueryPerformanceFrequency(&li))
    {
        std::cout << "QueryPerformanceFrequency failed!\n";
    }
    PCFreq = double(li.QuadPart) / 1000.0f;
    _qpcInited = true;
}
/***********************************************************************************/
/* End InitCounter                                                                 */
/***********************************************************************************/
/***********************************************************************************/
/* Clock                                                                           */
/***********************************************************************************/
static double CLOCK()
{
    if (!_qpcInited) InitCounter();
    LARGE_INTEGER li;
    QueryPerformanceCounter(&li);
    return double(li.QuadPart) / PCFreq;
}
/***********************************************************************************/
/* End Clock                                                                       */
/***********************************************************************************/
/***********************************************************************************/
/* Avgdur                                                                          */
/***********************************************************************************/
static double avgdur(double newdur)
{
    _avgdur = 0.98 * _avgdur + 0.02 * newdur;
    return _avgdur;
}
/***********************************************************************************/
/* End Avgdur                                                                      */
/***********************************************************************************/
/***********************************************************************************/
/* Avgfps                                                                          */
/***********************************************************************************/
static double avgfps()
{
    if (CLOCK() - _fpsstart > 1000)
    {
        _fpsstart = CLOCK();
        _avgfps = 0.7 * _avgfps + 0.3 * _fps1sec;
        _fps1sec = 0;
    }
    _fps1sec++;
    return _avgfps;
}
/***********************************************************************************/
/*  End Avgfps                                                                     */
/***********************************************************************************/
/***********************************************************************************/
/* Get Console Char                                                                */
/***********************************************************************************/
static bool getconchar(KEY_EVENT_RECORD& krec)
{
    DWORD cc;
    INPUT_RECORD irec;
    HANDLE h = GetStdHandle(STD_INPUT_HANDLE);

    if (h == NULL)
    {
        return false; // console not found
    }

    for (; ; )
    {
        ReadConsoleInput(h, &irec, 1, &cc);
        if (irec.EventType == KEY_EVENT
            && ((KEY_EVENT_RECORD&)irec.Event).bKeyDown
            )//&& ! ((KEY_EVENT_RECORD&)irec.Event).wRepeatCount )
        {
            krec = (KEY_EVENT_RECORD&)irec.Event;
            return true;
        }
    }
    return false; //future ????
}
/***********************************************************************************/
/* End Get Console Char                                                            */
/***********************************************************************************/
/***********************************************************************************/
/* Get File Name                                                                   */
/***********************************************************************************/
static bool GetFileName(Mode mode, char filename[MAX_PATH])
{
    TCHAR CWD[MAX_PATH];
    bool retval = true;
    OPENFILENAME ofn;
    TCHAR szFile[MAX_PATH];
    ZeroMemory(&szFile, sizeof(szFile));
    ZeroMemory(&ofn, sizeof(ofn));
    ofn.lStructSize = sizeof(ofn);
    ofn.hwndOwner = NULL;  // If you have a window to center over, put its HANDLE here
    if (mode == Mode::mImage_File) ofn.lpstrFilter = _T("Image Files\0*.png;*.jpg;*.tif;*.bmp;*.jpeg;*.gif\0Any File\0*.*\0");
    else if (mode == Mode::mPlayback_Video) ofn.lpstrFilter = _T("Video Files\0*.avi;*.mp4;*.webm;*.flv;*.mjpg;*.mjpeg\0Any File\0*.*\0");
    else ofn.lpstrFilter = _T("Text Files\0*.txt\0Any File\0*.*\0");
    ofn.lpstrFile = LPWSTR(szFile);
    ofn.nMaxFile = MAX_PATH;
    ofn.lpstrTitle = _T("Select a File, to Processs");
    ofn.Flags = OFN_DONTADDTORECENT | OFN_FILEMUSTEXIST;
    GetCurrentDirectory(MAX_PATH, CWD);
    if (GetOpenFileName(&ofn))
    {
        size_t output_size;
        wcstombs_s(&output_size, filename, MAX_PATH, ofn.lpstrFile, MAX_PATH);
    }
    else
    {
        // All this stuff below is to tell you exactly how you messed up above. 
        // Once you've got that fixed, you can often (not always!) reduce it to a 'user cancelled' assumption.
        switch (CommDlgExtendedError())
        {
        case CDERR_DIALOGFAILURE: std::cout << "CDERR_DIALOGFAILURE\n";   break;
        case CDERR_FINDRESFAILURE: std::cout << "CDERR_FINDRESFAILURE\n";  break;
        case CDERR_INITIALIZATION: std::cout << "CDERR_INITIALIZATION\n";  break;
        case CDERR_LOADRESFAILURE: std::cout << "CDERR_LOADRESFAILURE\n";  break;
        case CDERR_LOADSTRFAILURE: std::cout << "CDERR_LOADSTRFAILURE\n";  break;
        case CDERR_LOCKRESFAILURE: std::cout << "CDERR_LOCKRESFAILURE\n";  break;
        case CDERR_MEMALLOCFAILURE: std::cout << "CDERR_MEMALLOCFAILURE\n"; break;
        case CDERR_MEMLOCKFAILURE: std::cout << "CDERR_MEMLOCKFAILURE\n";  break;
        case CDERR_NOHINSTANCE: std::cout << "CDERR_NOHINSTANCE\n";     break;
        case CDERR_NOHOOK: std::cout << "CDERR_NOHOOK\n";          break;
        case CDERR_NOTEMPLATE: std::cout << "CDERR_NOTEMPLATE\n";      break;
        case CDERR_STRUCTSIZE: std::cout << "CDERR_STRUCTSIZE\n";      break;
        case FNERR_BUFFERTOOSMALL: std::cout << "FNERR_BUFFERTOOSMALL\n";  break;
        case FNERR_INVALIDFILENAME: std::cout << "FNERR_INVALIDFILENAME\n"; break;
        case FNERR_SUBCLASSFAILURE: std::cout << "FNERR_SUBCLASSFAILURE\n"; break;
        default: std::cout << "You cancelled.\n";
            retval = false;
        }
    }
    SetCurrentDirectory(CWD);
    return(retval);
}
/***********************************************************************************/
/* End Get File Name                                                               */
/***********************************************************************************/

/***********************************************************************************/
/* GetVideoMode                                                                    */
/***********************************************************************************/
static Mode GetVideoMode(void)
{
    KEY_EVENT_RECORD key;
    Mode mode = Mode::mNone;
    do
    {
        std::cout << "Select Live Video, PlayBack File or Image File" << std::endl;
        std::cout << "1 - Live Video" << std::endl;
        std::cout << "2 - PlayBack File" << std::endl;
        std::cout << "3 - Image File" << std::endl;
        std::cout << "E - Exit" << std::endl;

        getconchar(key);
        std::cout << key.uChar.AsciiChar << std::endl;
        if ((key.uChar.AsciiChar == 'E') || (key.uChar.AsciiChar == 'e')) break;
        else if (key.uChar.AsciiChar == '1') mode = Mode::mLive_Video;
        else if (key.uChar.AsciiChar == '2') mode = Mode::mPlayback_Video;
        else if (key.uChar.AsciiChar == '3') mode = Mode::mImage_File;
        else std::cout << "Invalid Input" << std::endl << std::endl;
    } while (mode == Mode::mNone);
    return(mode);
}
/***********************************************************************************/
/* End GetVideoMode                                                                */
/***********************************************************************************/
/***********************************************************************************/
/* GetVideoDevice                                                                */
/***********************************************************************************/
static int GetVideoDevice(void)
{
    int deviceID = -1;
    KEY_EVENT_RECORD key;
    int numdev;
    DeviceEnumerator de;
    std::map<int, Device> devices = de.getVideoDevicesMap();

    int* deviceid = new int[devices.size()];
    do {
        numdev = 0;
        std::cout << "Select video Device" << std::endl;
        for (auto const& device : devices)
        {
            deviceid[numdev] = device.first;
            std::cout << numdev + 1 << " - " << device.second.deviceName << std::endl;
            numdev++;
        }
        std::cout << "E - exit" << std::endl;
        getconchar(key);
        if ((key.uChar.AsciiChar == 'E') || (key.uChar.AsciiChar == 'e')) break;
        int value = static_cast<int>(key.uChar.AsciiChar) - 48;
        if ((value >= 1) && value <= numdev) deviceID = deviceid[value - 1];
        else std::cout << "Invalid Input" << std::endl << std::endl;
    } while (deviceID == -1);
    delete[] deviceid;
    return(deviceID);
}
/***********************************************************************************/
/* End GetVideoDevice                                                              */
/***********************************************************************************/
/***********************************************************************************/
/* GetVideoResolution                                                              */
/***********************************************************************************/
static VideoResolution GetVideoResolution(void)
{
    VideoResolution vres = VideoResolution::rNone;
    KEY_EVENT_RECORD key;
    do
    {
        std::cout << "Select Video Resolution" << std::endl;
        std::cout << "1 - 640 x 480" << std::endl;
        std::cout << "2 - 1280 x 720" << std::endl;
        std::cout << "E - Exit" << std::endl;

        getconchar(key);
        std::cout << key.uChar.AsciiChar << std::endl;
        if ((key.uChar.AsciiChar == 'E') || (key.uChar.AsciiChar == 'e')) break;
        else if (key.uChar.AsciiChar == '1') vres = VideoResolution::r640X480;
        else if (key.uChar.AsciiChar == '2') vres = VideoResolution::r1280X720;
        else std::cout << "Invalid Input" << std::endl << std::endl;
    } while (vres == VideoResolution::rNone);

    return(vres);
}
/***********************************************************************************/
/* End GetVideoResolution                                                          */
/***********************************************************************************/
/***********************************************************************************/
/* GetVideoSaveMode                                                            */
/***********************************************************************************/
static VideoSaveMode GetVideoSaveMode(void)
{
    VideoSaveMode videosavemode = VideoSaveMode::vNone;
    KEY_EVENT_RECORD key;
    do
    {
        std::cout << "Select Video Save Mode" << std::endl;
        std::cout << "1 - No Save" << std::endl;
        std::cout << "2 - Save" << std::endl;
        std::cout << "3 - Save With No ALPR" << std::endl;
        std::cout << "E - Exit" << std::endl;

        getconchar(key);
        std::cout << key.uChar.AsciiChar << std::endl;
        if ((key.uChar.AsciiChar == 'E') || (key.uChar.AsciiChar == 'e')) exit(0);
        else if (key.uChar.AsciiChar == '1') videosavemode = VideoSaveMode::vNoSave;
        else if (key.uChar.AsciiChar == '2') videosavemode = VideoSaveMode::vSave;
        else if (key.uChar.AsciiChar == '3') videosavemode = VideoSaveMode::vSaveWithNoALPR;
        else std::cout << "Invalid Input" << std::endl << std::endl;
    } while (videosavemode == VideoSaveMode::vNone);

    return(videosavemode);
}
/***********************************************************************************/
/* End GetVideoSaveMode                                                            */
/***********************************************************************************/
/***********************************************************************************/
/* GetResponses                                                                    */
/***********************************************************************************/

static void GetResponses(LPVOID param)
{
    ClgdemowDlg* pMain = (ClgdemowDlg*)param;
    ssize_t BytesRead;
    ssize_t BytesOnSocket = 0;
    while ((BytesOnSocket = BytesAvailableTcp(TcpConnectedPort)) > 0)
    {
        if (BytesOnSocket < 0) return;
        if (BytesOnSocket > BytesNeeded) BytesOnSocket = BytesNeeded;
        BytesRead = ReadDataTcp(TcpConnectedPort, (unsigned char*)&ResponseBuffer[BytesInResponseBuffer], BytesOnSocket);
        if (BytesRead <= 0)
        {
            printf("Read Response Error - Closing Socket\n");
            CloseTcpConnectedPort(&TcpConnectedPort);
        }
        BytesInResponseBuffer += BytesRead;

        if (BytesInResponseBuffer == BytesNeeded)
        {
            if (GetResponseMode == ResponseMode::ReadingHeader)
            {
                memcpy(&RespHdrNumBytes, ResponseBuffer, sizeof(RespHdrNumBytes));
                RespHdrNumBytes = ntohs(RespHdrNumBytes);
                GetResponseMode = ResponseMode::ReadingMsg;
                BytesNeeded = RespHdrNumBytes;
                BytesInResponseBuffer = 0;
            }
            else if (GetResponseMode == ResponseMode::ReadingMsg)
            {
             
                //pMain->AppendLineToMultilineEditCtrl(pMain->m_description, GetWC(ResponseBuffer));
               
                printf("Response %s\n", ResponseBuffer);
                writeMessage(m_hPipe, GetWC(ResponseBuffer));
                GetResponseMode = ResponseMode::ReadingHeader;
                BytesInResponseBuffer = 0;
                BytesNeeded = sizeof(RespHdrNumBytes);
            }
        }
    }
    if (BytesOnSocket < 0)
    {
        printf("Read Response Error - Closing Socket\n");
        CloseTcpConnectedPort(&TcpConnectedPort);
    }
}

/***********************************************************************************/
/* End GetResponses                                                                */
/***********************************************************************************/

void ClgdemowDlg::OnClose()
{
    // When everything done, release the video capture and write object
    cap.release();
    if (videosavemode != VideoSaveMode::vNoSave)  outputVideo.release();

    // Closes all the frames
    destroyAllWindows();

    // TODO: 여기에 메시지 처리기 코드를 추가 및/또는 기본값을 호출합니다.

    CDialogEx::OnClose();
}

UINT ThreadForLiveCam(LPVOID param)
{
    ClgdemowDlg* pMain = (ClgdemowDlg*)param;
    std::string county;
    county = "us";

    Alpr alpr(county, "");
    alpr.setTopN(2);
    if (alpr.isLoaded() == false)
    {
        std::cerr << "Error loading OpenALPR" << std::endl;
        //return 1;
    }
    while (pMain->m_isWorkingThread_LiveCam)
    {
        Mat frame;
        double start = CLOCK();
        // Capture frame-by-frame
        if (mode == Mode::mImage_File)
        {
            frame = imread(filename);
        }
        else cap >> frame;

        // 
        // If the frame is empty, break immediately
        if (frame.empty())
            break;

        if (frameno == 0) motiondetector.ResetMotionDetection(&frame);
        if (videosavemode != VideoSaveMode::vSaveWithNoALPR)
        {
            detectandshow(&alpr, frame, "", false);
            //GetResponses(pMain);

            cv::putText(frame, text,
                cv::Point(10, frame.rows - 10), //top-left position
                FONT_HERSHEY_COMPLEX_SMALL, 0.5,
                Scalar(0, 255, 0), 0, LINE_AA, false);

        }

        // Write the frame into the file 'outcpp.avi'
        if (videosavemode != VideoSaveMode::vNoSave)
        {
            outputVideo.write(frame);
        }

        // Display the resulting frame    
        //imshow("Frame", frame);

        //cvtColor(frame, frame, COLOR_BGR2GRAY);



        //화면에 보여주기 위한 처리입니다.
        int bpp = 8 * frame.elemSize();
        assert((bpp == 8 || bpp == 24 || bpp == 32));

        int padding = 0;
        //32 bit image is always DWORD aligned because each pixel requires 4 bytes
        if (bpp < 32)
            padding = 4 - (frame.cols % 4);

        if (padding == 4)
            padding = 0;

        int border = 0;
        //32 bit image is always DWORD aligned because each pixel requires 4 bytes
        if (bpp < 32)
        {
            border = 4 - (frame.cols % 4);
        }

        Mat mat_temp;
        if (border > 0 || frame.isContinuous() == false)
        {
            // Adding needed columns on the right (max 3 px)
            cv::copyMakeBorder(frame, mat_temp, 0, 0, 0, border, cv::BORDER_CONSTANT, 0);
        }
        else
        {
            mat_temp = frame;
        }

        RECT r;
        pMain->m_PIC.GetClientRect(&r);
        cv::Size winSize(r.right, r.bottom);

        pMain->cimage_mfc.Create(winSize.width, winSize.height, 24);

        BITMAPINFO* bitInfo = (BITMAPINFO*)malloc(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
        bitInfo->bmiHeader.biBitCount = bpp;
        bitInfo->bmiHeader.biWidth = mat_temp.cols;
        bitInfo->bmiHeader.biHeight = -mat_temp.rows;
        bitInfo->bmiHeader.biPlanes = 1;
        bitInfo->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
        bitInfo->bmiHeader.biCompression = BI_RGB;
        bitInfo->bmiHeader.biClrImportant = 0;
        bitInfo->bmiHeader.biClrUsed = 0;
        bitInfo->bmiHeader.biSizeImage = 0;
        bitInfo->bmiHeader.biXPelsPerMeter = 0;
        bitInfo->bmiHeader.biYPelsPerMeter = 0;

        if (bpp == 8)
        {
            RGBQUAD* palette = bitInfo->bmiColors;
            for (int i = 0; i < 256; i++)
            {
                palette[i].rgbBlue = palette[i].rgbGreen = palette[i].rgbRed = (BYTE)i;
                palette[i].rgbReserved = 0;
            }
        }

        // Image is bigger or smaller than into destination rectangle
        // we use stretch in full rect

        if (mat_temp.cols == winSize.width && mat_temp.rows == winSize.height)
        {
            // source and destination have same size
            // transfer memory block
            // NOTE: the padding border will be shown here. Anyway it will be max 3px width

            SetDIBitsToDevice(pMain->cimage_mfc.GetDC(),
                //destination rectangle
                0, 0, winSize.width, winSize.height,
                0, 0, 0, mat_temp.rows,
                mat_temp.data, bitInfo, DIB_RGB_COLORS);
        }
        else
        {
            // destination rectangle
            int destx = 0, desty = 0;
            int destw = winSize.width;
            int desth = winSize.height;

            // rectangle defined on source bitmap
            // using imgWidth instead of mat_temp.cols will ignore the padding border
            int imgx = 0, imgy = 0;
            int imgWidth = mat_temp.cols - border;
            int imgHeight = mat_temp.rows;

            StretchDIBits(pMain->cimage_mfc.GetDC(),
                destx, desty, destw, desth,
                imgx, imgy, imgWidth, imgHeight,
                mat_temp.data, bitInfo, DIB_RGB_COLORS, SRCCOPY);
        }

        HDC dc = ::GetDC(pMain->m_PIC.m_hWnd);
        pMain->cimage_mfc.BitBlt(dc, 0, 0);
        ::ReleaseDC(pMain->m_PIC.m_hWnd, dc);

        pMain->cimage_mfc.ReleaseDC();
        pMain->cimage_mfc.Destroy();

        double dur = CLOCK() - start;
        sprintf_s(text, "avg time per frame %f ms. fps %f. frameno = %d", avgdur(dur), avgfps(), frameno++);

    }

    return 0;
}


UINT ThreadForPlayBack(LPVOID param)
{
    ClgdemowDlg* pMain = (ClgdemowDlg*)param;
    std::string county;
    county = "us";

    Alpr alpr(county, "");
    alpr.setTopN(2);
    if (alpr.isLoaded() == false)
    {
        std::cerr << "Error loading OpenALPR" << std::endl;
        //return 1;
    }
    while (pMain->m_isWorkingThread_playback)
    {
        Mat frame;
        double start = CLOCK();
        // Capture frame-by-frame
        if (mode == Mode::mImage_File)
        {
            frame = imread(filename);
        }
        else cap >> frame;

        // 
        // If the frame is empty, break immediately
        if (frame.empty())
            break;

        if (frameno == 0) motiondetector.ResetMotionDetection(&frame);
        if (videosavemode != VideoSaveMode::vSaveWithNoALPR)
        {
            detectandshow(&alpr, frame, "", false);
            //GetResponses(pMain);

            cv::putText(frame, text,
                cv::Point(10, frame.rows - 10), //top-left position
                FONT_HERSHEY_COMPLEX_SMALL, 0.5,
                Scalar(0, 255, 0), 0, LINE_AA, false);

        }

        // Write the frame into the file 'outcpp.avi'
        if (videosavemode != VideoSaveMode::vNoSave)
        {
            outputVideo.write(frame);
        }

        // Display the resulting frame    
        //imshow("Frame", frame);

        //cvtColor(frame, frame, COLOR_BGR2GRAY);



        //화면에 보여주기 위한 처리입니다.
        int bpp = 8 * frame.elemSize();
        assert((bpp == 8 || bpp == 24 || bpp == 32));

        int padding = 0;
        //32 bit image is always DWORD aligned because each pixel requires 4 bytes
        if (bpp < 32)
            padding = 4 - (frame.cols % 4);

        if (padding == 4)
            padding = 0;

        int border = 0;
        //32 bit image is always DWORD aligned because each pixel requires 4 bytes
        if (bpp < 32)
        {
            border = 4 - (frame.cols % 4);
        }

        Mat mat_temp;
        if (border > 0 || frame.isContinuous() == false)
        {
            // Adding needed columns on the right (max 3 px)
            cv::copyMakeBorder(frame, mat_temp, 0, 0, 0, border, cv::BORDER_CONSTANT, 0);
        }
        else
        {
            mat_temp = frame;
        }

        RECT r;
        pMain->m_PIC.GetClientRect(&r);
        cv::Size winSize(r.right, r.bottom);

        pMain->cimage_mfc.Create(winSize.width, winSize.height, 24);

        BITMAPINFO* bitInfo = (BITMAPINFO*)malloc(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
        bitInfo->bmiHeader.biBitCount = bpp;
        bitInfo->bmiHeader.biWidth = mat_temp.cols;
        bitInfo->bmiHeader.biHeight = -mat_temp.rows;
        bitInfo->bmiHeader.biPlanes = 1;
        bitInfo->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
        bitInfo->bmiHeader.biCompression = BI_RGB;
        bitInfo->bmiHeader.biClrImportant = 0;
        bitInfo->bmiHeader.biClrUsed = 0;
        bitInfo->bmiHeader.biSizeImage = 0;
        bitInfo->bmiHeader.biXPelsPerMeter = 0;
        bitInfo->bmiHeader.biYPelsPerMeter = 0;

        if (bpp == 8)
        {
            RGBQUAD* palette = bitInfo->bmiColors;
            for (int i = 0; i < 256; i++)
            {
                palette[i].rgbBlue = palette[i].rgbGreen = palette[i].rgbRed = (BYTE)i;
                palette[i].rgbReserved = 0;
            }
        }

        // Image is bigger or smaller than into destination rectangle
        // we use stretch in full rect

        if (mat_temp.cols == winSize.width && mat_temp.rows == winSize.height)
        {
            // source and destination have same size
            // transfer memory block
            // NOTE: the padding border will be shown here. Anyway it will be max 3px width

            SetDIBitsToDevice(pMain->cimage_mfc.GetDC(),
                //destination rectangle
                0, 0, winSize.width, winSize.height,
                0, 0, 0, mat_temp.rows,
                mat_temp.data, bitInfo, DIB_RGB_COLORS);
        }
        else
        {
            // destination rectangle
            int destx = 0, desty = 0;
            int destw = winSize.width;
            int desth = winSize.height;

            // rectangle defined on source bitmap
            // using imgWidth instead of mat_temp.cols will ignore the padding border
            int imgx = 0, imgy = 0;
            int imgWidth = mat_temp.cols - border;
            int imgHeight = mat_temp.rows;

            StretchDIBits(pMain->cimage_mfc.GetDC(),
                destx, desty, destw, desth,
                imgx, imgy, imgWidth, imgHeight,
                mat_temp.data, bitInfo, DIB_RGB_COLORS, SRCCOPY);
        }

        HDC dc = ::GetDC(pMain->m_PIC.m_hWnd);
        pMain->cimage_mfc.BitBlt(dc, 0, 0);
        ::ReleaseDC(pMain->m_PIC.m_hWnd, dc);

        pMain->cimage_mfc.ReleaseDC();
        pMain->cimage_mfc.Destroy();
       
        double dur = CLOCK() - start;
        sprintf_s(text, "avg time per frame %f ms. fps %f. frameno = %d", avgdur(dur), avgfps(), frameno++);

    }

    return 0;
}

void ClgdemowDlg::OnBnClickedButton_playback()
{
    mode = Mode::mPlayback_Video;
    if (mode == Mode::mNone) exit(0);

    if (GetFileName(mode, filename)) std::cout << "Filename is " << filename << std::endl;
    else return;
	vres = VideoResolution::r640X480;// GetVideoResolution();
	if (vres == VideoResolution::rNone) exit(0);
	videosavemode = VideoSaveMode::vNoSave;
	cap.open(filename);
	if (!cap.isOpened()) {
		cout << "Error opening video file" << endl;
		//return -1;
	}
    cap.set(cv::CAP_PROP_FRAME_WIDTH, 640);
    cap.set(cv::CAP_PROP_FRAME_HEIGHT, 480);
    if (mode != Mode::mImage_File)
    {
        // Default resolutions of the frame are obtained.The default resolutions are system dependent.
        int frame_width = (int)cap.get(cv::CAP_PROP_FRAME_WIDTH);
        int frame_height = (int)cap.get(cv::CAP_PROP_FRAME_HEIGHT);
        printf("Frame width= %d height=%d\n", frame_width, frame_height);
       
    }
    //Thread Running 
    m_isWorkingThread_playback = true;
    m_isWorkingThread_LiveCam = false;
    m_pThread_playback = AfxBeginThread(ThreadForPlayBack, this);
}

void ClgdemowDlg::AppendTextToEditCtrl(CEdit& edit, LPCTSTR pszText)
{
    // get the initial text length
    int nLength = edit.GetWindowTextLength();
    // put the selection at the end of text
    edit.SetSel(nLength, nLength);
    // replace the selection
    edit.ReplaceSel(pszText);
}
void ClgdemowDlg::AppendLineToMultilineEditCtrl(CEdit& edit, LPCTSTR pszText)
{
    CString strLine;
    // add CR/LF to text
    strLine.Format(_T("\r\n%s"), pszText);
    AppendTextToEditCtrl(edit, strLine);
}
void ClgdemowDlg::OnBnClickedButton_LiveCam()
{
    

    mode = Mode::mLive_Video;
	if (mode == Mode::mNone) exit(0);


    deviceID = 0;
	if (deviceID == -1) exit(0);
    vres = VideoResolution::r640X480;
	if (vres == VideoResolution::rNone) exit(0);

	videosavemode = VideoSaveMode::vNoSave;
	// open selected camera using selected API
	cap.open(deviceID, apiID);
	if (!cap.isOpened()) {
		cout << "Error opening video stream" << endl;

	}
	cap.set(cv::CAP_PROP_FRAME_WIDTH, 640);
	cap.set(cv::CAP_PROP_FRAME_HEIGHT, 480);


	if (mode != Mode::mImage_File)
	{
		// Default resolutions of the frame are obtained.The default resolutions are system dependent.
		int frame_width = (int)cap.get(cv::CAP_PROP_FRAME_WIDTH);
		int frame_height = (int)cap.get(cv::CAP_PROP_FRAME_HEIGHT);
		printf("Frame width= %d height=%d\n", frame_width, frame_height);

	}
	//Thread Running 
    m_isWorkingThread_playback = false;
	m_isWorkingThread_LiveCam = true;
	m_pThread_playback = AfxBeginThread(ThreadForLiveCam, this);

}


