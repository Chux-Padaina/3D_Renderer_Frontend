module ImporterPanel

open System
open System.IO
open System.Windows.Forms
open System.Drawing
open LogPanel
open ViewerPanel  // to call createGameObjectFromFiles

let mutable filePanel = new Panel()

// Temporary storage for paths
let mutable meshPath = ""
let mutable diffPath = ""
let mutable normPath = ""
let mutable metalPath = ""
let mutable roughPath = ""
let mutable step = 0

let instructionLabel = new Label(Text = "Click Import to begin")
instructionLabel.Dock <- DockStyle.Top
instructionLabel.Height <- 25
instructionLabel.TextAlign <- ContentAlignment.MiddleCenter
instructionLabel.Font <- new Font("Consolas", 10.0f, FontStyle.Bold)
instructionLabel.BackColor <- Color.FromArgb(40, 40, 40)
instructionLabel.ForeColor <- Color.White

let updateInstruction () =
    instructionLabel.Text <-
        match step with
        | 0 -> "Select mesh file (.fbx or .obj)"
        | 1 -> "Select diffuse texture"
        | 2 -> "Select normal map"
        | 3 -> "Select metallic map"
        | 4 -> "Select roughness map"
        | 5 -> "Enter number of objects to add"
        | _ -> "Import complete."

let createImporterPanel () =
    filePanel <- new Panel()
    filePanel.Dock <- DockStyle.Fill
    filePanel.BackColor <- Color.DarkGray

    let importButton = new Button(Text = "Import", Font = new Font("Consolas", 14.0f, FontStyle.Bold))
    importButton.Dock <- DockStyle.Fill

    let escButton = new Button(Text = "Esc", Height = 40, Font = new Font("Consolas", 10.0f, FontStyle.Bold))
    escButton.Dock <- DockStyle.Top

    let pathTextBox = new TextBox()
    pathTextBox.Dock <- DockStyle.Top
    pathTextBox.Height <- 30
    pathTextBox.ReadOnly <- true

    let fileListBox = new ListBox()
    fileListBox.Dock <- DockStyle.Fill
    fileListBox.Font <- new Font("Consolas", 10.0f)

    let instanceCountBox = new NumericUpDown()
    instanceCountBox.Dock <- DockStyle.Top
    instanceCountBox.Minimum <- 1m
    instanceCountBox.Maximum <- 1000m
    instanceCountBox.Value <- 1m
    instanceCountBox.Visible <- false

    let confirmButton = new Button(Text = "Confirm", Height = 40, Font = new Font("Consolas", 10.0f, FontStyle.Bold))
    confirmButton.Dock <- DockStyle.Top
    confirmButton.Visible <- false

    let fileBrowserPanel = new Panel()
    fileBrowserPanel.Dock <- DockStyle.Fill
    fileBrowserPanel.Controls.Add(fileListBox)
    fileBrowserPanel.Controls.Add(pathTextBox)
    fileBrowserPanel.Controls.Add(escButton)
    fileBrowserPanel.Controls.Add(instructionLabel)

    let resetState () =
        meshPath <- ""; diffPath <- ""; normPath <- ""; metalPath <- ""; roughPath <- ""
        step <- 0
        instanceCountBox.Visible <- false
        confirmButton.Visible <- false
        updateInstruction ()

    let showImportMenu () =
        filePanel.Controls.Clear()
        filePanel.Controls.Add(importButton)
        resetState ()

    let showFileBrowser () =
        filePanel.Controls.Clear()
        filePanel.Controls.Add(fileBrowserPanel)
        updateInstruction ()

    let loadDirectory (path: string) =
        try
            fileListBox.Items.Clear()
            if Directory.GetParent(path) <> null then
                fileListBox.Items.Add("[..]") |> ignore
            for dir in Directory.GetDirectories(path) do
                fileListBox.Items.Add("[D] " + Path.GetFileName(dir)) |> ignore
            for file in Directory.GetFiles(path) do
                fileListBox.Items.Add(Path.GetFileName(file)) |> ignore
            pathTextBox.Text <- path
        with _ -> ()

    importButton.Click.Add (fun _ ->
        showFileBrowser()
        loadDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
    )

    escButton.Click.Add (fun _ ->
        showImportMenu()
    )

    fileListBox.DoubleClick.Add (fun _ ->
        if fileListBox.SelectedItem <> null then
            let selectedItem = fileListBox.SelectedItem.ToString()
            let currentPath = pathTextBox.Text
            if selectedItem = "[..]" then
                let parent = Directory.GetParent(currentPath)
                if parent <> null then loadDirectory(parent.FullName)
            elif selectedItem.StartsWith("[D] ") then
                let newPath = Path.Combine(currentPath, selectedItem.Substring(4))
                loadDirectory(newPath)
            else
                let fullPath = Path.Combine(currentPath, selectedItem)
                match step with
                | 0 -> meshPath <- fullPath
                | 1 -> diffPath <- fullPath
                | 2 -> normPath <- fullPath
                | 3 -> metalPath <- fullPath
                | 4 ->
                    roughPath <- fullPath
                    // Show instance count input
                    fileBrowserPanel.Controls.Add(instanceCountBox)
                    fileBrowserPanel.Controls.Add(confirmButton)
                    instanceCountBox.Visible <- true
                    confirmButton.Visible <- true
                | _ -> ()

                step <- step + 1
                updateInstruction()
    )

    confirmButton.Click.Add (fun _ ->
        let instanceCount = int instanceCountBox.Value
        appendLog $"Creating {instanceCount} objects with:\nMesh: {meshPath}\nDiff: {diffPath}\nNorm: {normPath}\nMetal: {metalPath}\nRough: {roughPath}"

        for _ in 1 .. instanceCount do
            ViewerPanel.createGameObjectFromFiles "ImportedObj" meshPath diffPath normPath metalPath roughPath

        showImportMenu()
    )

    showImportMenu()
    filePanel
