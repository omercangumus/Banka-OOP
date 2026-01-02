Set WshShell = CreateObject("WScript.Shell") 
WshShell.Run "D:\postgreSQL\bin\pg_ctl.exe -D ""D:\postgreSQL\data"" -l ""c:\Users\omerc\Desktop\BankaBenim\persistent_db.log"" start", 0, False
