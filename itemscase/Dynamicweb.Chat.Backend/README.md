How to build
============

1. Open solution
2. Add reference to lib/Dynamicweb.dll
3. Open Package Manager Console (Tools > Library Package Manager > Package Manager Console)
4. Install-Package Microsoft.AspNet.SignalR.SystemWeb [https://nuget.org/packages/Microsoft.AspNet.SignalR.SystemWeb/]
5. Build
6. Copy all dll files from bin folder into solution bin folder, e.g.
		copy /y Dynamicweb.Chat.Backend\bin\*.dll c:\dynamicweb.net\Application(8.2.1.5)\bin
7. Copy all dll files from lib folder into solution bin folder, e.g.
		copy /y lib\*.dll c:\dynamicweb.net\Application(8.2.1.5)\bin
8. Create a page named "Chat history" (on Area 1) for storing the chat sessions and messages

Happy chatting!
