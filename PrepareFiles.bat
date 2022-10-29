@echo off

set STRIP=%LINUX_MULTIARCH_ROOT%x86_64-unknown-linux-gnu\bin\x86_64-unknown-linux-gnu-strip.exe
set SRC_ROOT_DIR=E:\UT4Source\repo
set DST_ROOT_DIR=E:\UT4Source\UT4UU\Source\Programs\Installer\Files

set SRC_DIR=%SRC_ROOT_DIR%\UnrealTournament\Plugins\UT4UU\Binaries
set DST_DIR=%DST_ROOT_DIR%\UnrealTournament\Plugins\UT4UU\Binaries

copy %SRC_DIR%\..\UT4UU.uplugin %DST_DIR%\..\UT4UU.uplugin

%STRIP% -d -o %DST_DIR%\Linux\libUE4-Funchook-Linux-Shipping.so %SRC_DIR%\Linux\libUE4-Funchook-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4Server-Funchook-Linux-Shipping.so %SRC_DIR%\Linux\libUE4Server-Funchook-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4-UT4UUHelper-Linux-Shipping.so %SRC_DIR%\Linux\libUE4-UT4UUHelper-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4Server-UT4UUHelper-Linux-Shipping.so %SRC_DIR%\Linux\libUE4Server-UT4UUHelper-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4-UT4UU-Linux-Shipping.so %SRC_DIR%\Linux\libUE4-UT4UU-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4Server-UT4UU-Linux-Shipping.so %SRC_DIR%\Linux\libUE4Server-UT4UU-Linux-Shipping.so
copy %SRC_DIR%\Win64\UE4-Funchook-Win64-Shipping.dll %DST_DIR%\Win64\UE4-Funchook-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4-UT4UUHelper-Win64-Shipping.dll %DST_DIR%\Win64\UE4-UT4UUHelper-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4-UT4UU-Win64-Shipping.dll %DST_DIR%\Win64\UE4-UT4UU-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Server-Funchook-Win64-Shipping.dll %DST_DIR%\Win64\UE4Server-Funchook-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Server-UT4UUHelper-Win64-Shipping.dll %DST_DIR%\Win64\UE4Server-UT4UUHelper-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Server-UT4UU-Win64-Shipping.dll %DST_DIR%\Win64\UE4Server-UT4UU-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Editor-Funchook.dll %DST_DIR%\Win64\UE4Editor-Funchook.dll
copy %SRC_DIR%\Win64\UE4Editor-UT4UUHelper.dll %DST_DIR%\Win64\UE4Editor-UT4UUHelper.dll
copy %SRC_DIR%\Win64\UE4Editor-UT4UU.dll %DST_DIR%\Win64\UE4Editor-UT4UU.dll

set SRC_DIR=%SRC_ROOT_DIR%\Engine\Binaries
set DST_DIR=%DST_ROOT_DIR%\Engine\Binaries

%STRIP% -d -o %DST_DIR%\Linux\libUE4-SSL-Linux-Shipping.so %SRC_DIR%\Linux\libUE4-SSL-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4Server-SSL-Linux-Shipping.so %SRC_DIR%\Linux\libUE4Server-SSL-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4-HTTP-Linux-Shipping.so %SRC_DIR%\Linux\libUE4-HTTP-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4Server-HTTP-Linux-Shipping.so %SRC_DIR%\Linux\libUE4Server-HTTP-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4-HttpNetworkReplayStreaming-Linux-Shipping.so %SRC_DIR%\Linux\libUE4-HttpNetworkReplayStreaming-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4Server-HttpNetworkReplayStreaming-Linux-Shipping.so %SRC_DIR%\Linux\libUE4Server-HttpNetworkReplayStreaming-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4-XMPP-Linux-Shipping.so %SRC_DIR%\Linux\libUE4-XMPP-Linux-Shipping.so
%STRIP% -d -o %DST_DIR%\Linux\libUE4Server-XMPP-Linux-Shipping.so %SRC_DIR%\Linux\libUE4Server-XMPP-Linux-Shipping.so
copy %SRC_DIR%\Win64\UE4-SSL-Win64-Shipping.dll %DST_DIR%\Win64\UE4-SSL-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4-HTTP-Win64-Shipping.dll %DST_DIR%\Win64\UE4-HTTP-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4-HttpNetworkReplayStreaming-Win64-Shipping.dll %DST_DIR%\Win64\UE4-HttpNetworkReplayStreaming-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4-XMPP-Win64-Shipping.dll %DST_DIR%\Win64\UE4-XMPP-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Server-SSL-Win64-Shipping.dll %DST_DIR%\Win64\UE4Server-SSL-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Server-HTTP-Win64-Shipping.dll %DST_DIR%\Win64\UE4Server-HTTP-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Server-HttpNetworkReplayStreaming-Win64-Shipping.dll %DST_DIR%\Win64\UE4Server-HttpNetworkReplayStreaming-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Server-XMPP-Win64-Shipping.dll %DST_DIR%\Win64\UE4Server-XMPP-Win64-Shipping.dll
copy %SRC_DIR%\Win64\UE4Editor-SSL.dll %DST_DIR%\Win64\UE4Editor-SSL.dll
copy %SRC_DIR%\Win64\UE4Editor-HTTP.dll %DST_DIR%\Win64\UE4Editor-HTTP.dll
copy %SRC_DIR%\Win64\UE4Editor-HttpNetworkReplayStreaming.dll %DST_DIR%\Win64\UE4Editor-HttpNetworkReplayStreaming.dll
copy %SRC_DIR%\Win64\UE4Editor-XMPP.dll %DST_DIR%\Win64\UE4Editor-XMPP.dll