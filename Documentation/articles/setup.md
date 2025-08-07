<h1>Seeting it up</h1>

An installation of the .NET Framework 4.8 is needed. Following NuGets are needed an can be installed through the NuGet Explorer:

- YamlDotNet (by Antoine Aubry)
- GraphX (by panthernet)
- MathNet.Numerics (by Christoph Ruegg, Marcus Cuda, Jurgen Van Gael)
- MathNet.Numerics.MKL.Win (by Christoph Ruegg, Marcus Cuda, Jurgen Van Gael)
- MathNet.Numerics.Data.Matlab (by Christoph Ruegg, Marcus Cuda, Jurgen Van Gael)
- Microsoft.Windows.SDK.Contracts (by Microsoft)
- Newtonsoft.Json (by James Newton-King)
- WebSocketSharp (by sta)
- WebSocketSharp-netstandard (by sta)

If you do not plan to use Delsys sensors, you can safely delete or exclude the Delsys folder from your solution.
This avoids any build errors related to Delsys-specific code.

If you do plan to use Delsys sensors, you need to install the following Delsys API NuGet packages manually:

- Delsys.Driver
- DelsysAPI
- Signals

### Steps to Install the Delsys API for .NET (optional)

Follow these instructions to set up the necessary Delsys drivers and libraries so you can build and run projects that depend on the Delsys Trigno system.



#### 1. Install Trignio Discover

1. **Download and install [Trignio Discover](https://delsys.com/activation/).**  
   - You do **not** need to use the software itself.  
   - Installing Trignio Discover is required because it provides the **USB driver** needed for Delsys devices.



#### 2. Download the Delsys Trignio API

1. **Clone or download** the [Delsys Trignio API repository](https://github.com/delsys-inc/Example-Applications) from GitHub.  
2. **Unzip** the folder to a convenient location on your machine.

You now have local copies of the sample applications and the **NuGet packages** for Delsys.



#### 3. Add the Delsys API to Your .NET Solution (e.g., imBlocks)

1. **Open your solution** in Visual Studio.  
2. **Open the NuGet Package Manager**:
   - Go to **Tools** > **NuGet Package Manager** > **Manage NuGet Packages for Solution...**  
   - If you don’t see the NuGet Package Manager, install it via the **Visual Studio Installer**.
3. **Open NuGet Settings**:
   - In the **Package Manager** window, click the **Settings (gear icon)** in the top-right corner.
4. **Add a new package source**:
   1. Click the green **plus (+)** sign to add a new source.
   2. Give it a **Name** (for example, `DelsysLocal`).
   3. Click the **...** button to browse to the path where you unzipped the Delsys repo.  
      - Navigate to:  
        `Example-Applications > C# .NET > DelsysAPI`
   4. Select this folder and click **OK**.
5. **Enable prerelease packages**:
   - Check the box for **Include prerelease** in the NuGet Package Manager.
   - Set **Package source** to **All** (or the new source you just created).
6. **Verify the Delsys packages**:
   - Make sure the following are **installed** in your solution:
     - **Delsys.Drivers**  
     - **DelsysAPI**  
     - **Signals**  



#### 4. Compile the Solution

After adding these sources and ensuring the Delsys packages are installed, **rebuild** your solution. It should now compile without errors, assuming all other dependencies are in place.

## Building and Running MOSAIC

1. **Open** the `MOSAIC.sln` solution in Visual Studio.
2. **Build** both Debug and Release configurations.
   - NuGet packages are restored automatically.
3. The main library is `mosaic_libary` (no Main function).
4. Try the main project `mosaic_program` (Main function in Program.cs):
   - When prompted, select an example YAML file.
   - You’ll see the interactive block table and control panels.

