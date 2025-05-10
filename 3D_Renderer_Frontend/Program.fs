open System
open System.Windows.Forms
open System.Drawing
open System.IO
open System.Runtime.InteropServices
open System.Threading

open ImporterPanel
open InspectorPanel
open LogPanel
open ViewerPanel

// Native DLL handling
[<DllImport("kernel32.dll", SetLastError = true)>]
extern bool SetDllDirectory(string lpPathName)

[<DllImport("kernel32.dll", SetLastError = true)>]
extern bool LoadLibrary(string lpFileName)

[<EntryPoint>]
let main argv =
    try
        // Set DLL path
        let dllFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenGLRenderEngine")
        let dllPath = Path.Combine(dllFolder, "OpenGLRenderEngine.dll")

        if not (Directory.Exists(dllFolder)) then
            MessageBox.Show($"DLL folder not found: {dllFolder}") |> ignore
            failwithf "DLL folder missing: %s" dllFolder

        ignore (SetDllDirectory(dllFolder))

        if not (LoadLibrary(dllPath)) then
            MessageBox.Show($"Failed to load DLL manually: {dllPath}") |> ignore
            failwithf "Failed to load DLL: %s" dllPath

        // Main Form
        let form = new Form()
        form.Text <- "3D Renderer Frontend"
        form.Width <- 1920
        form.Height <- 1080
        form.KeyPreview <- true // Ensures form catches key input before controls

        // Layout: Left-Right
        let outerSplit = new SplitContainer()
        outerSplit.Dock <- DockStyle.Fill
        outerSplit.Orientation <- Orientation.Vertical

        // Left: Viewer + Logs
        let leftSplit = new SplitContainer()
        leftSplit.Dock <- DockStyle.Fill
        leftSplit.Orientation <- Orientation.Horizontal

        // Right: Inspector + Importer
        let rightSplit = new SplitContainer()
        rightSplit.Dock <- DockStyle.Fill
        rightSplit.Orientation <- Orientation.Horizontal

        // Panels
        let viewerPanel = ViewerPanel.createViewerPanel()
        let logPanel = LogPanel.createLogPanel()
        let inspectorPanel = InspectorPanel.createInspectorPanel()
        let importerPanel = ImporterPanel.createImporterPanel()

        // Left setup
        leftSplit.Panel1.Controls.Add(viewerPanel)
        leftSplit.Panel2.Controls.Add(logPanel)

        // Right setup
        rightSplit.Panel1.Controls.Add(inspectorPanel)
        rightSplit.Panel2.Controls.Add(importerPanel)

        // Attach
        outerSplit.Panel1.Controls.Add(leftSplit)
        outerSplit.Panel2.Controls.Add(rightSplit)
        form.Controls.Add(outerSplit)

        // On Load
        form.Load.AddHandler(EventHandler(fun _ _ ->
            outerSplit.SplitterDistance <- int (float form.ClientSize.Width * 0.66)
            leftSplit.SplitterDistance <- int (float form.ClientSize.Height * 0.66)
            rightSplit.SplitterDistance <- int (float form.ClientSize.Height * 0.5)

            outerSplit.IsSplitterFixed <- true
            leftSplit.IsSplitterFixed <- true
            rightSplit.IsSplitterFixed <- true

            InspectorPanel.adjustSizes()
            ViewerPanel.initializeRenderer leftSplit.Panel1.Width leftSplit.Panel1.Height
            ViewerPanel.getPictureBox().Focus()

            // High-FPS Render Thread
            let renderThread = new Thread(ThreadStart(fun () ->
                let sw = Diagnostics.Stopwatch.StartNew()
                while not form.IsDisposed do
                    form.Invoke(fun _ ->
                        LogPanel.updateFPSAndLogs leftSplit.Panel1.Width leftSplit.Panel1.Height
                        ViewerPanel.updateRender()
                    ) |> ignore
                    Thread.Sleep(0) // Yield CPU to avoid 100% usage
            ))
            renderThread.IsBackground <- true
            renderThread.Start()
        ))

        Application.Run(form)
        0

    with ex ->
        MessageBox.Show("Startup failure: " + ex.Message) |> ignore
        1
