using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using NLog;


namespace WindowsForms_client
{

    enum reqFuncType
    {
        Auth,
        Plate,
        OTPReqeust
    };
    enum ServerStatusCode
    {
        OK=200,
        NG=900,
    };
    public partial class MainFrm : Form
    {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        WebRequestHandler clientHandler;
        Thread IPCListener;
        string tokenVal = null;
        const int BUFFER_SIZE = 4096;  
        const string serverURL = "https://10.58.0.154:8443";
        Form2 otpInput = null;
        public string receivedOTPNumber;
        System.Windows.Forms.Timer loginWating = null;
        private bool bLoginPASS = false;
        class ResponseAPI
        {
            public string id { get; set; }
            public string plateNum { get; set; }

        }
        public class receivedMSG
        {
            public int code;
            public string msg;
            public string token;

        }
        public class receivedPlateInfo
        {
            public string licensenumber; //#1
            public string licensestatus; //#2
            public string licenseexpdate; //#3
            public string ownername;//#4
            public string ownerbirthday; //#5
            public string owneraddress;//#6
            public string ownercity;//#7
            public string vhemake;//#8
            public string vhemodel; // #9
            public string vhecolor; //#10

        }
        public class queryAccount
        {
            public string userid;
            public string password;
        }
        public class queryPlate
        {
            public string plateNum;
            public string id;
        }
        public class queryOTP
        {
            public string userid;
            public string otpKey;
        }
        class Hash
        {
            public static string getHashSha256(string text)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                SHA256Managed hashstring = new SHA256Managed();
                byte[] hash = hashstring.ComputeHash(bytes);
                string hashString = string.Empty;
                foreach (byte x in hash)
                {
                    hashString += String.Format("{0:x2}", x);
                }
                return hashString;
            }
        }
        public MainFrm()
        {

            InitializeComponent();
            setCertification();
            loginWating = new System.Windows.Forms.Timer();
            loginWating.Tick += LoginWating_Tick;
            loginWating.Interval = 50;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoginWating_Tick(object sender, EventArgs e)
        {
            progressBar1.Increment(1);
            if (progressBar1.Value >= progressBar1.Maximum)
                loginWating.Stop();

        }
        private static X509Certificate2 GetCertificateFromStore(string certName)
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            try
            {

                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, false);
                if (signingCert.Count == 0)
                    return null;
                return signingCert[0];
              
            }
            finally
            {
                store.Close();
            }
        }
        private void setCertification()
        {
            clientHandler = new WebRequestHandler();
            string certName = "CN=CLIENT, OU=CMU22_DEF, O=LGE, L=PGH, S=PA, C=US";
            X509Certificate2 cert = GetCertificateFromStore(certName);
            if (cert == null)
            {
                MessageBox.Show("Client must have specific certification to connect server.\nPlease contact server administrator to gain certifiacion", "Certification Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
            else
            {
                clientHandler.ClientCertificates.Add(cert);
                clientHandler.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequired;
                clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            }
         
        }

        private Task<HttpResponseMessage> HttpPostMessageSecure(reqFuncType type, string plateNum = null, string userid = null, string password = null, int timeout = 300, string optNum = null)
        {
            HttpResponseMessage response = null;
            try
            {

                string urlAddress = serverURL;
                switch (type)
                {
                    case reqFuncType.Auth:
                        urlAddress += "/auth/login";
                        break;
                    case reqFuncType.Plate:
                        urlAddress += "/db/vehicle";
                        break;
                    case reqFuncType.OTPReqeust:
                        urlAddress += "/auth/otp-check";
                        break;
                    default:
                        break;
                }
                ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpClient _httpClient = new HttpClient(clientHandler);
                _httpClient.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
                _httpClient.BaseAddress = new Uri(urlAddress);
                string bearer = "Bearer " + tokenVal;
                _httpClient.DefaultRequestHeaders.Add("Authorization", bearer);
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //Request
                queryAccount accountInfo = null;
                queryPlate plateInfo = null;
                queryOTP OTPInfo = null;
                Logger.Info($"[Request to Server : RequestType:{type} Addr:{_httpClient.BaseAddress}");
                switch (type)
                {
                    case reqFuncType.Auth:
                        accountInfo = new queryAccount();
                        accountInfo.userid = userid;
                        accountInfo.password = password; //Hash.getHashSha256(password);
                        response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, accountInfo).Result;
                        break;
                    case reqFuncType.Plate:
                        plateInfo = new queryPlate();
                        plateInfo.id = "user";
                        plateInfo.plateNum = plateNum;
                        response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, plateInfo).Result;
                        break;
                    case reqFuncType.OTPReqeust:
                        OTPInfo = new queryOTP();
                        OTPInfo.userid = userid;
                        OTPInfo.otpKey = receivedOTPNumber;
                        response = _httpClient.PostAsJsonAsync(_httpClient.BaseAddress, OTPInfo).Result;
                        break;
                    default:
                        break;

                }
                Logger.Info($"[Response StatusCode] : {response.StatusCode}"); 
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //response 
                    var contents = response.Content.ReadAsStringAsync();
                    receivedMSG returnMSG = null;
                    this.Invoke(new Action(delegate ()
                    {
                        Logger.Info(contents.Result.ToString());

                    }));
                    switch (type)
                    {
                        case reqFuncType.Auth:
                            returnMSG = response.Content.ReadAsAsync<receivedMSG>().Result;
                            Logger.Info($"[Response from Server] RequestType:{type} StatusCode:{response.StatusCode} StatusMessage:{returnMSG.msg} {returnMSG.code}");
                            if (returnMSG.code == (int)ServerStatusCode.OK)
                            {
                                this.Invoke(new Action(delegate ()
                                {
                                    bLoginPASS = true;
                                    TXT_DESC.AppendText("1st Log-In OK" + Environment.NewLine);
                                }));
                            }
                               
                            else
                            {
                                this.Invoke(new Action(delegate ()
                                {
                                    TXT_DESC.AppendText("Log In fail" + Environment.NewLine);
                                }));
                                

                            }
                            break;
                        case reqFuncType.Plate:
                            
                            var recevJsonPlate = response.Content.ReadAsAsync<receivedPlateInfo>().Result;
                            if (String.IsNullOrEmpty(recevJsonPlate.licensestatus))
                                break;
                            this.Invoke(new Action(delegate ()
                            {
                                TXT_DESC.AppendText("Target Matching: " + recevJsonPlate.licensenumber + Environment.NewLine);
                                TXT_DESC.AppendText("Status: " + recevJsonPlate.licensestatus + Environment.NewLine);
                                dataGridView1.Rows.Add(recevJsonPlate.licensenumber,
                                    recevJsonPlate.licensestatus, recevJsonPlate.licenseexpdate,
                                    recevJsonPlate.ownername, recevJsonPlate.ownerbirthday, recevJsonPlate.owneraddress,
                                    recevJsonPlate.ownercity, recevJsonPlate.vhemake, recevJsonPlate.vhemodel, recevJsonPlate.vhecolor);
                            }));
                            break;
                        case reqFuncType.OTPReqeust:
                            returnMSG = response.Content.ReadAsAsync<receivedMSG>().Result;
                            Logger.Info($"[Response from Server : {type} {response.StatusCode} {returnMSG.msg} {returnMSG.code}");
                            if (returnMSG.code == (int)ServerStatusCode.OK)
                            {
                                tokenVal = returnMSG.token;
                                this.Invoke(new Action(delegate ()
                                {
                                    TXT_ID.Enabled = false;
                                    TXT_PW.Enabled = false;
                                    BTN_LOGIN.Enabled = false;
                                    TXT_DESC.AppendText("2FA OTP OK" + Environment.NewLine);
                                    TXT_DESC.AppendText("Success to login!!!" + Environment.NewLine);
                                }));
                                loadALPR();
                                MessageBox.Show("Success to login!!!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                               
                            }
                            else if(returnMSG.code == (int)ServerStatusCode.NG)
                            {
                                this.Invoke(new Action(delegate ()
                                {
                                    TXT_DESC.AppendText("Your OTP number is wrong" + Environment.NewLine);
                                }));
                             
                                MessageBox.Show("Your OTP number is wrong!!!\nPlease confirm your OTP number","OTP Number",MessageBoxButtons.OK,MessageBoxIcon.Information);
                            }
                            else
                            {
                                this.Invoke(new Action(delegate ()
                                {
                                    TXT_DESC.AppendText("Your OTP number is wrong" + Environment.NewLine);
                                }));

                                MessageBox.Show("Your OTP number is wrong!!!\nPlease confirm your OTP number", "OTP Number", MessageBoxButtons.OK, MessageBoxIcon.Information);
                               
                            }
                            break;
                        default:
                            break;
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return Task.FromResult(response);

        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        
        void listenALPR()
        {
            bool bResult;
            String strPipeName = String.Format(@"\\{0}\pipe\{1}",
                ".",                
                "IPC_DATA_CHANNEL"        
                );

            IntPtr pSa = IntPtr.Zero;  
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();

            SECURITY_DESCRIPTOR sd;
            SecurityNative.InitializeSecurityDescriptor(out sd, 1);
            SecurityNative.SetSecurityDescriptorDacl(ref sd, true, IntPtr.Zero, false);
            sa.lpSecurityDescriptor = Marshal.AllocHGlobal(Marshal.SizeOf(
                typeof(SECURITY_DESCRIPTOR)));
            Marshal.StructureToPtr(sd, sa.lpSecurityDescriptor, false);
            sa.bInheritHandle = false;            
            sa.nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
            pSa = Marshal.AllocHGlobal(sa.nLength);
            Marshal.StructureToPtr(sa, pSa, false);

            IntPtr hPipe = PipeNative.CreateNamedPipe(
                strPipeName,                        
                PipeOpenMode.PIPE_ACCESS_DUPLEX,    
                PipeMode.PIPE_TYPE_MESSAGE |        
                PipeMode.PIPE_READMODE_MESSAGE |    
                PipeMode.PIPE_WAIT,                 
                PipeNative.PIPE_UNLIMITED_INSTANCES,
                BUFFER_SIZE,                        
                BUFFER_SIZE,                        
                PipeNative.NMPWAIT_USE_DEFAULT_WAIT,
                pSa                                 
                );

            if (hPipe.ToInt32() == PipeNative.INVALID_HANDLE_VALUE)
            {
                Console.WriteLine("Unable to create named pipe {0} w/err 0x{1:X}",
                    strPipeName, PipeNative.GetLastError());
                return;
            }
            Console.WriteLine("The named pipe, {0}, is created.", strPipeName);
            Console.WriteLine("Waiting for the client's connection...");

            bool bConnected = PipeNative.ConnectNamedPipe(hPipe, IntPtr.Zero) ?
                true : PipeNative.GetLastError() == PipeNative.ERROR_PIPE_CONNECTED;

            if (!bConnected)
            {
                Console.WriteLine(
                    "Error occurred while connecting to the client: 0x{0:X}",
                    PipeNative.GetLastError());
                PipeNative.CloseHandle(hPipe);     
                return;
            }

            while (true)
            {
                string strMessage;
                byte[] bRequest = new byte[BUFFER_SIZE];    
                uint cbBytesRead, cbRequestBytes;
                cbRequestBytes = BUFFER_SIZE;
                bResult = PipeNative.ReadFile(     
                    hPipe,                         
                    bRequest,                      
                    cbRequestBytes,                
                    out cbBytesRead,               
                    IntPtr.Zero);                  

                if (!bResult/*Failed*/ || cbBytesRead == 0/*Finished*/)
                {
                    IPCListener.Abort();
                    Application.ExitThread();
                    Environment.Exit(0);
                }

                strMessage = Encoding.Unicode.GetString(bRequest).TrimEnd('\0');
                Console.WriteLine("Receives {0} bytes; Message: \"{1}\"",
                    cbBytesRead, strMessage);
                string[] platenum = strMessage.Split('\n');
                if (platenum.Length > 0)
                {
                    this.Invoke(new Action(delegate ()
                    {
                        Logger.Info("Plate Number: " + platenum[0] + Environment.NewLine);
                    }));
                    Task.Run(() =>
                    {
                        if (platenum[0].Length >= 6)
                            HttpPostMessageSecure(reqFuncType.Plate, platenum[0]);
                    });


                }

            }
            PipeNative.FlushFileBuffers(hPipe);
            PipeNative.DisconnectNamedPipe(hPipe);
            PipeNative.CloseHandle(hPipe);
        }
        private static void EncryptFile(string inFile, RSA rsaPublicKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                using (ICryptoTransform transform = aes.CreateEncryptor())
                {
                    RSAPKCS1KeyExchangeFormatter keyFormatter = new RSAPKCS1KeyExchangeFormatter(rsaPublicKey);
                    byte[] keyEncrypted = keyFormatter.CreateKeyExchange(aes.Key, aes.GetType());
                    byte[] LenK = new byte[4];
                    byte[] LenIV = new byte[4];
                    int lKey = keyEncrypted.Length;
                    LenK = BitConverter.GetBytes(lKey);
                    int lIV = aes.IV.Length;
                    LenIV = BitConverter.GetBytes(lIV);
                    string encrFolder = Path.GetDirectoryName(inFile);
                    int startFileName = inFile.LastIndexOf("\\") + 1;
                    string outFile = encrFolder + "\\" + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";

                    using (FileStream outFs = new FileStream(outFile, FileMode.CreateNew))
                    {
                        outFs.Write(LenK, 0, 4);
                        outFs.Write(LenIV, 0, 4);
                        outFs.Write(keyEncrypted, 0, lKey);
                        outFs.Write(aes.IV, 0, lIV);

                        using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                        {
                            int count = 0;
                            int blockSizeBytes = aes.BlockSize / 8;
                            byte[] data = new byte[blockSizeBytes];
                            int bytesRead = 0;

                            using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                            {
                                do
                                {
                                    count = inFs.Read(data, 0, blockSizeBytes);
                                    outStreamEncrypted.Write(data, 0, count);
                                    bytesRead += count;
                                }
                                while (count > 0);
                                inFs.Close();
                            }
                            outStreamEncrypted.FlushFinalBlock();
                            outStreamEncrypted.Close();
                        }
                        outFs.Close();
                        File.Delete(inFile);
                    }
                }
            }
        }
        private static void DecryptFile(string inFile, RSA rsaPrivateKey)
        {

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;

                byte[] LenK = new byte[4];
                byte[] LenIV = new byte[4];

                string decrFolder = Path.GetDirectoryName(inFile);
                int startFileName = inFile.LastIndexOf("\\") + 1;

                string outFile = decrFolder + "\\" + Path.GetFileNameWithoutExtension(inFile) + ".csv";
                using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                {

                    inFs.Seek(0, SeekOrigin.Begin);
                    inFs.Seek(0, SeekOrigin.Begin);
                    inFs.Read(LenK, 0, 3);
                    inFs.Seek(4, SeekOrigin.Begin);
                    inFs.Read(LenIV, 0, 3);

                    int lenK = BitConverter.ToInt32(LenK, 0);
                    int lenIV = BitConverter.ToInt32(LenIV, 0);

                    int startC = lenK + lenIV + 8;
                    int lenC = (int)inFs.Length - startC;

                    byte[] KeyEncrypted = new byte[lenK];
                    byte[] IV = new byte[lenIV];

                    inFs.Seek(8, SeekOrigin.Begin);
                    inFs.Read(KeyEncrypted, 0, lenK);
                    inFs.Seek(8 + lenK, SeekOrigin.Begin);
                    inFs.Read(IV, 0, lenIV);
                    Directory.CreateDirectory(decrFolder);
                    byte[] KeyDecrypted = rsaPrivateKey.Decrypt(KeyEncrypted, RSAEncryptionPadding.Pkcs1);

                    using (ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV))
                    {

                        using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                        {

                            int count = 0;

                            int blockSizeBytes = aes.BlockSize / 8;
                            byte[] data = new byte[blockSizeBytes];

                            inFs.Seek(startC, SeekOrigin.Begin);
                            using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                            {
                                do
                                {
                                    count = inFs.Read(data, 0, blockSizeBytes);
                                    outStreamDecrypted.Write(data, 0, count);
                                }
                                while (count > 0);

                                outStreamDecrypted.FlushFinalBlock();
                                outStreamDecrypted.Close();
                            }
                            outFs.Close();
                        }
                        inFs.Close();
                        File.Delete(inFile);
                    }
                }
            }
        }

        private void menu_fileEncrypt()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV (*.csv)|*.csv";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string certName = "CN=CLIENT, OU=CMU22_DEF, O=LGE, L=PGH, S=PA, C=US";
                    X509Certificate2 cert = GetCertificateFromStore(certName);
                    if (cert == null)
                    {
                        Logger.Info("Certificate not found.\n");
                        return;
                    }

                    EncryptFile(ofd.FileName, (RSA)cert.PublicKey.Key);
                    MessageBox.Show("Success to encrypt file!!!","Encrypt File",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    this.Invoke(new Action(delegate ()
                    {
                        TXT_DESC.AppendText("Success to encrypt file!!!" + Environment.NewLine);
                    }));
                    

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                
            }
        }
        private void menu_fileDecrypt()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "enc (*.enc)|*.enc";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string certName = "CN=CLIENT, OU=CMU22_DEF, O=LGE, L=PGH, S=PA, C=US";
                    X509Certificate2 cert = GetCertificateFromStore(certName);
                    if (cert == null)
                    {
                        Logger.Info("Certificate not found.\n");
                        return;
                    }

                    DecryptFile(ofd.FileName, cert.GetRSAPrivateKey());
                    MessageBox.Show("Success to decrypt file!!!", "Decrypt File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Invoke(new Action(delegate ()
                    {
                        TXT_DESC.AppendText("Success to encrypt file!!!" + Environment.NewLine);
                    }));
               
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
        }
        private void menu_savetoFile()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "Output.csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            int columnCount = dataGridView1.Columns.Count;
                            string columnNames = "";
                            string[] outputCsv = new string[dataGridView1.Rows.Count + 1];
                            for (int i = 0; i < columnCount; i++)
                            {
                                columnNames += dataGridView1.Columns[i].HeaderText.ToString() + ",";
                            }
                            outputCsv[0] += columnNames;

                            for (int i = 1; (i - 1) < dataGridView1.Rows.Count; i++)
                            {
                                for (int j = 0; j < columnCount; j++)
                                {
                                    outputCsv[i] += dataGridView1.Rows[i - 1].Cells[j].Value.ToString() + ",";
                                }
                            }

                            File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                            MessageBox.Show("Data Exported Successfully !!!", "Save File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Invoke(new Action(delegate ()
                            {
                                TXT_DESC.AppendText("Data Exported Successfully !!!" + Environment.NewLine);
                            }));
                           
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

            Application.ExitThread();
            Process[] processList = Process.GetProcessesByName("lgdemo_w");

            if (processList.Length > 0)
            {
                processList[0].Kill();
            }
            Environment.Exit(0);

        }
        private void menu_Logout()
        {

            this.Invoke(new Action(delegate ()
            {
                BTN_LOGIN.Enabled = true;
                TXT_ID.Enabled = true;
                TXT_PW.Enabled = true;
                progressBar1.Value = 0;
                TXT_DESC.Clear();
                TXT_ID.Clear();
                TXT_PW.Clear();
                tokenVal = "";
                Process[] processList = Process.GetProcessesByName("lgdemo_w");

                if (processList.Length > 0)
                {
                    processList[0].Kill();
                }
                panel2.Visible = false;
                panel1.Visible = true;
                
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();

                IPCListener.Abort();

            }));

        }
        private void menuToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            menuToolStripMenuItem.HideDropDown();
            if (e.ClickedItem.Text.ToUpper() == "SAVE")
            {
                menu_savetoFile();
            }
            else if (e.ClickedItem.Text.ToUpper() == "LOGOUT")
            {
                menu_Logout();
                MessageBox.Show("Success to logout!!!","Logout",MessageBoxButtons.OK,MessageBoxIcon.Information);
                this.Invoke(new Action(delegate ()
                {
                    TXT_DESC.AppendText("Success to logout!!!" + Environment.NewLine);
                }));
                
            }
            else if (e.ClickedItem.Text.ToUpper() == "FILE ENCRYPT")
            {
                menu_fileEncrypt();
            }
            else if (e.ClickedItem.Text.ToUpper() == "FILE DECRYPT")
            {
                menu_fileDecrypt();
            }

        }

        private async void BTN_LOGIN_Click(object sender, EventArgs e)
        {

            if(TXT_ID.Text.Length == 0 || TXT_PW.Text.Length == 0)
            {
                this.Invoke(new Action(delegate ()
                {
                    TXT_DESC.AppendText("Login to fail : input validation" + Environment.NewLine);
                }));
                MessageBox.Show("Please input your ID and password", "Log-In", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            this.Invoke(new Action(delegate ()
            {
                this.progressBar1.Value = 0;
                this.progressBar1.Maximum = 150;
            }));
            loginWating.Start();
            await Task.Run(() =>
            {
                HttpPostMessageSecure(reqFuncType.Auth, null, TXT_ID.Text, TXT_PW.Text, 20000);
            });

            this.Invoke(new Action(delegate ()
            {
                loginWating.Stop();
                this.progressBar1.Value = progressBar1.Maximum;

            }));

            if (bLoginPASS)
            {
                if (otpInput == null)
                {
                    otpInput = new Form2(receivedOTPNumber);
                    otpInput.Owner = this;
                    if (otpInput.ShowDialog() == DialogResult.OK)
                    {
                        await Task.Run(() =>
                        {
                            HttpPostMessageSecure(reqFuncType.OTPReqeust, null, TXT_ID.Text, null, 10000, receivedOTPNumber);
                        });

                    }
                }
                else
                {
                    if (otpInput.ShowDialog() == DialogResult.OK)
                    {
                        await Task.Run(() =>
                        {
                            HttpPostMessageSecure(reqFuncType.OTPReqeust, null, TXT_ID.Text, null, 10, receivedOTPNumber);

                        });
                    }
                }
            }
            else
            {
                MessageBox.Show("Log-In Failed\nPlease check your ID and password", "Log-In", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            bLoginPASS = false;



        }
        private void loadALPR()
        {
            IPCListener = new Thread(listenALPR);
            IPCListener.Start();
            this.Invoke(new Action(delegate ()
            {
                panel2.Visible = true;
               
            }));
            using (var process = new Process())
            {
                process.StartInfo.FileName = "lgdemo_w.exe";
                process.StartInfo.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                if (File.Exists(Path.Combine(process.StartInfo.WorkingDirectory, "lgdemo_w.exe")))
                {
                    process.Start();
                    this.Invoke(new Action(delegate ()
                    {
                        IntPtr ptr = IntPtr.Zero;
                        while ((ptr = process.MainWindowHandle) == IntPtr.Zero) ;
                        SetParent(process.MainWindowHandle, panel2.Handle);
                        MoveWindow(process.MainWindowHandle, 0, 0, panel2.Width, panel2.Height, true);

                    }));

                }
                else
                {
                    MessageBox.Show("Not load loadALPR\nPlease check to exist lgdemo_w.exe","LoadALPR",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }

            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(IPCListener != null)
                IPCListener.Abort();

        }
       
    }
}
