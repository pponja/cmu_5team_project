
// lgdemo_wDlg.h: 헤더 파일
//

#pragma once


// ClgdemowDlg 대화 상자
class ClgdemowDlg : public CDialogEx
{
// 생성입니다.
public:
	
	ClgdemowDlg(CWnd* pParent = nullptr);	// 표준 생성자입니다.
	
	//Mat m_matImage;
	BITMAPINFO* m_pBitmapInfo; // Bitmap 정보를 담고 있는 구조체.
	
	void CreateBitmapInfo(int w, int h, int bpp); // Bitmap 정보를 생성하는 함수.
	void DrawImage(); // 그리는 작업을 수행하는 함수.
// 대화 상자 데이터입니다.
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_LGDEMO_W_DIALOG };
#endif

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV 지원입니다.


// 구현입니다.
protected:
	HICON m_hIcon;

	// 생성된 메시지 맵 함수
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	void AppendTextToEditCtrl(CEdit& edit, LPCTSTR pszText);
	void AppendLineToMultilineEditCtrl(CEdit& edit, LPCTSTR pszText);
	CWinThread* m_pThread_playback;
	bool m_isWorkingThread_playback = FALSE;
	CWinThread* m_pThread_LiveCam = FALSE; 
	bool m_isWorkingThread_LiveCam;
	CImage cimage_mfc;
	CStatic m_picture;
	CStatic m_Pic;
	afx_msg void OnDestroy();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	CStatic m_PIC;
	CEdit m_TXT_ID;
	CEdit m_TXT_PW;
	CButton btn_LogIn;
	CButton Btn_playback;
	CButton Btn_LiveCam;
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	CEdit m_description;
	afx_msg void OnClose();
	afx_msg void OnBnClickedButton_playback();
	afx_msg void OnBnClickedButton_LiveCam();
};
