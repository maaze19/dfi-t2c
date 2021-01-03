# dfi-t2c
 Converts dfi tokens to coins

1. Download and build the code with visual studio
2. Start the defid service on your computer (not the defi-app)
3. Start dfi-t2c.exe
    - Based on your config file of dfi-t2c (dfi-t2c.dll.config), the command will ask you before swapping tokens to coins
    
## Configuration options

Usually it should work with the default settings on Windows-PCs, but you can change all settings for your needs:

```xml
<configuration>
  <appSettings>
    <add key="DaemonUrl" value="http://127.0.0.1:8555" />
    <add key="CookiePath" value="%APPDATA%/DeFi Blockchain/.cookie" />
    <add key="MinimumConvertingAmount" value="1" />
    <add key="RpcRequestTimeoutInSeconds" value="10" />
    <add key="WaitForInputBeforeExit" value="true" />
    <add key="AskBeforeChange" value="true" />
  </appSettings>
</configuration>
```

### DeamonUrl (string)
Defines the url for the defid RPC api
### CookiePath (string)
Defines the path to the .cookie file which will be created by defid on startup for authentication. The format is `username:password`. If you want using rpcUser and rpcPassword, just create a file and write your rpc username and password in the same format and change the path to your newly created file.
### MinimumConvertingAmount (decimal)
Defines the minimum dfi tokens per address which are neccessary to start a swap on this address.
### RpcRequestTimeoutInSeconds (short)
Defines how long the system will wait for a response from the rpc server (defid) in seconds.
### WaitForInputBeforeExit (bool)
If `true`, the console program will be asking for pressing any key before closing. If `false` the system will close the app immediately after everything is done.
### AskBeforeChange (bool)
If `true`, the app asks whether it should swap the tokens. If `false` the app will swap the tokens automatically if the amount per address exceeds `MinimumConvertingAmount`
