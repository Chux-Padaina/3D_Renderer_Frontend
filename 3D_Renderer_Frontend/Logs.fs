module LogPanel

open System
open System.Windows.Forms
open System.Drawing

let mutable logBox = new TextBox()
let mutable fpsLabel = new Label()

let createLogPanel () =
    let panel = new Panel()
    panel.Dock <- DockStyle.Fill
    panel.BackColor <- Color.FromArgb(35, 35, 35)
    panel.Padding <- Padding(5)

    fpsLabel <- new Label()
    fpsLabel.Dock <- DockStyle.Top
    fpsLabel.ForeColor <- Color.White
    fpsLabel.BackColor <- Color.FromArgb(30, 30, 30)
    fpsLabel.Font <- new Font("Consolas", 10.0f)
    fpsLabel.TextAlign <- ContentAlignment.MiddleLeft
    fpsLabel.Height <- 25
    fpsLabel.Text <- "FPS: 0"

    logBox <- new TextBox()
    logBox.Multiline <- true
    logBox.Dock <- DockStyle.Fill
    logBox.BackColor <- Color.FromArgb(30, 30, 30)
    logBox.ForeColor <- Color.White
    logBox.BorderStyle <- BorderStyle.None
    logBox.Font <- new Font("Consolas", 10.0f)
    logBox.ReadOnly <- true
    logBox.ScrollBars <- ScrollBars.Vertical

    panel.Controls.Add(logBox)
    panel.Controls.Add(fpsLabel)
    panel

let mutable lastFrameTime = DateTime.UtcNow

let updateFPSOnly (width: int) (height: int) =
    let now = DateTime.UtcNow
    let deltaTime = (now - lastFrameTime).TotalSeconds
    lastFrameTime <- now
    let fps = if deltaTime > 0.0 then 1.0 / deltaTime else 0.0
    fpsLabel.Text <- sprintf "Render Size: %dx%d | FPS: %.1f" width height fps

let updateFPSAndLogs (width: int) (height: int) =
    updateFPSOnly width height
    logBox.Text <- "[Logs disabled]"

let dumpLogsToFile () =
    () 

let appendLog (text: string) =
    logBox.AppendText("\r\n" + text)
