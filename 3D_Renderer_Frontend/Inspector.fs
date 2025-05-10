module InspectorPanel

open System
open System.Windows.Forms
open System.Drawing
open System.Collections.Generic
open NativeBindings


// Define the ObjectData type
type ObjectData = {
    Name: string
    Position: float32 * float32 * float32
    Rotation: float32 * float32 * float32
    Scale: float32 * float32 * float32
}



let objectDataList = ResizeArray<ObjectData>()
let objectPtrs = ResizeArray<nativeint>()


// Panels and Controls
let mutable inspectorSplit = new SplitContainer()
let mutable inspectorLayout = new FlowLayoutPanel()
let mutable objectListBox = new ListBox()

let mutable nameBox = new TextBox()
let mutable posXBox, posYBox, posZBox = new TextBox(), new TextBox(), new TextBox()
let mutable rotXBox, rotYBox, rotZBox = new TextBox(), new TextBox(), new TextBox()
let mutable scaleXBox, scaleYBox, scaleZBox = new TextBox(), new TextBox(), new TextBox()

// Helper to safely parse float from textbox
let tryParseFloat (text: string) =
    match Single.TryParse(text) with
    | (true, value) -> value
    | _ -> 0.0f

// Function to apply changes from textboxes into objectDataList
let applyChanges () =
    let index = objectListBox.SelectedIndex
    if index >= 0 && index < objectDataList.Count then
        let updatedObj = {
            Name = nameBox.Text
            Position = (tryParseFloat posXBox.Text, tryParseFloat posYBox.Text, tryParseFloat posZBox.Text)
            Rotation = (tryParseFloat rotXBox.Text, tryParseFloat rotYBox.Text, tryParseFloat rotZBox.Text)
            Scale = (tryParseFloat scaleXBox.Text, tryParseFloat scaleYBox.Text, tryParseFloat scaleZBox.Text)
        }

        objectDataList.[index] <- updatedObj
        objectListBox.Items.[index] <- updatedObj.Name

        let ptr = objectPtrs.[index]
        let (px, py, pz) = updatedObj.Position
        let (rx, ry, rz) = updatedObj.Rotation
        let (sx, sy, sz) = updatedObj.Scale

        _moveGameObject(ptr, px, py, pz)
        _rotateGameObject(ptr, rx, ry, rz)
        _scaleGameObject(ptr, sx, sy, sz)


// Public API to add object to the inspector panel
let addObjectToInspector (obj: ObjectData, ptr: nativeint) =
    objectDataList.Add(obj)
    objectPtrs.Add(ptr)
    objectListBox.Items.Add(obj.Name) |> ignore


// Create the Inspector Panel
let createInspectorPanel () =
    inspectorSplit <- new SplitContainer()
    inspectorSplit.Dock <- DockStyle.Fill
    inspectorSplit.Orientation <- Orientation.Horizontal
    inspectorSplit.SplitterDistance <- 300
    inspectorSplit.Panel1.BackColor <- Color.FromArgb(20, 20, 50)

    // Inspector Layout (top)
    inspectorLayout <- new FlowLayoutPanel()
    inspectorLayout.Dock <- DockStyle.Fill
    inspectorLayout.FlowDirection <- FlowDirection.TopDown
    inspectorLayout.WrapContents <- false
    inspectorLayout.AutoScroll <- true
    inspectorLayout.BackColor <- Color.Transparent

    let inspectorFont = new Font("Consolas", 9.0f)

    let createLabel (text: string) =
        let lbl = new Label()
        lbl.Text <- text
        lbl.AutoSize <- true
        lbl.ForeColor <- Color.White
        lbl.Font <- inspectorFont
        lbl

    let createTextBox () =
        let tb = new TextBox()
        tb.Width <- 50
        tb.Font <- inspectorFont
        tb

    nameBox <- new TextBox()
    nameBox.Width <- 180
    nameBox.Font <- inspectorFont

    posXBox <- createTextBox()
    posYBox <- createTextBox()
    posZBox <- createTextBox()

    rotXBox <- createTextBox()
    rotYBox <- createTextBox()
    rotZBox <- createTextBox()

    scaleXBox <- createTextBox()
    scaleYBox <- createTextBox()
    scaleZBox <- createTextBox()

    let createRow (title: string) (xBox: TextBox) (yBox: TextBox) (zBox: TextBox) =
        let panel = new FlowLayoutPanel()
        panel.FlowDirection <- FlowDirection.LeftToRight
        panel.WrapContents <- false
        panel.AutoSize <- true
        panel.Controls.Add(createLabel title)
        panel.Controls.Add(createLabel "X:")
        panel.Controls.Add(xBox)
        panel.Controls.Add(createLabel "Y:")
        panel.Controls.Add(yBox)
        panel.Controls.Add(createLabel "Z:")
        panel.Controls.Add(zBox)
        panel

    inspectorLayout.Controls.Clear()
    inspectorLayout.Controls.Add(createLabel "Name:")
    inspectorLayout.Controls.Add(nameBox)
    inspectorLayout.Controls.Add(createRow "Position " posXBox posYBox posZBox)
    inspectorLayout.Controls.Add(createRow "Rotation " rotXBox rotYBox rotZBox)
    inspectorLayout.Controls.Add(createRow "Scale    " scaleXBox scaleYBox scaleZBox)

    inspectorSplit.Panel1.Controls.Add(inspectorLayout)

    // Object List (bottom)
    objectListBox <- new ListBox()
    objectListBox.Dock <- DockStyle.Fill
    objectListBox.Font <- new Font("Consolas", 12.0f)
    inspectorSplit.Panel2.Controls.Add(objectListBox)

    objectListBox.SelectedIndexChanged.AddHandler(EventHandler(fun _ _ ->
        let index = objectListBox.SelectedIndex
        if index >= 0 && index < objectDataList.Count then
            let obj = objectDataList.[index]
            nameBox.Text <- obj.Name

            let (px, py, pz) = obj.Position
            posXBox.Text <- px.ToString()
            posYBox.Text <- py.ToString()
            posZBox.Text <- pz.ToString()

            let (rx, ry, rz) = obj.Rotation
            rotXBox.Text <- rx.ToString()
            rotYBox.Text <- ry.ToString()
            rotZBox.Text <- rz.ToString()

            let (sx, sy, sz) = obj.Scale
            scaleXBox.Text <- sx.ToString()
            scaleYBox.Text <- sy.ToString()
            scaleZBox.Text <- sz.ToString()
    ))

    nameBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    posXBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    posYBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    posZBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    rotXBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    rotYBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    rotZBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    scaleXBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    scaleYBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))
    scaleZBox.TextChanged.AddHandler(EventHandler(fun _ _ -> applyChanges()))

    inspectorSplit

let adjustSizes () =
    inspectorSplit.SplitterDistance <- inspectorSplit.Height / 2
