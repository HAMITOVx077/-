Add-Type -Path "C:\Users\texav\source\repos\МаршрутСборки\МаршрутСборки\bin\Debug\net8.0-windows\BCrypt.Net-Next.dll"
1..10 | % { [BCrypt.Net.BCrypt]::HashPassword("123456") }
