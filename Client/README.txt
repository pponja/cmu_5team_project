[How to build and run the client app]

- Execution environment
 1. Visual Studio 2022 Community
    - Installing a .NET 4.7.2 Frame from the Visual Studio Installer
      Modify > Individual Components > ".NET Framework 4.7.2 Targeting Pack"
 2. Change the window display setting DPI to 100%

 3. Build as below through ALPR_Client folder
   1) Double-click the client key file to install as default (File : Client.pfx, Password: qwe123.. , Location: ..\ALPR_Client\Key)
   2) Make a web server and run server (Refer to Server build Guide) and check the server IP
   3) Open the OpenALPR.sln solution (Location: ..\ALPR_Client)     
   4) Check the existence of lgdemo_w (MFC project) and WindowsForms_Clien (C# project) projects in Solution
   5) Start build (Ref. Necessary libraries and reference links are made inside, build should be successful if the folder configuration is not changed)
   7) Do not change or delete file in debug/release folder without execution file (location : ..\ALPR_Client\WindowsForms_client\bin\Debug) 
   6) Designate WindowsForms_Client as the startup project
   7) Change the serverURL value on line 58 of the form1.cs to your server IP obtained in 2) (as it is when configured as a local server)
   8) Check whether the Windows client app is running normally by executing the build (ALPRClient.exe)
