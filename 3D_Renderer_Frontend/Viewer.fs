module ViewerPanel

open System
open System.Drawing
open System.Windows.Forms
open System.Drawing.Imaging
open System.Runtime.InteropServices
open System.IO
open NativeBindings
open NativeUtils
open LogPanel
open System.Numerics
open InspectorPanel

let mutable pictureBox = new PictureBox()
let mutable rendererPtr = nativeint 0
let mutable framePtr = nativeint 0
let mutable cameraPtr = nativeint 0

let mutable gameObjectGCHandle: GCHandle option = None
let gameObjectPtrs = ResizeArray<nativeint>()

let mutable hasDumpedLogs = false

let mutable cameraPitch = 0.0f
let mutable cameraYaw = 0.0f

// FPS camera mode control
let mutable isFPSMode = false
let mutable screenCenter = Point.Empty

let mutable cameraX = 0.0f
let mutable cameraY = 20.0f
let mutable cameraZ = 20.0f

let mutable moveForward = false
let mutable moveBackward = false
let mutable moveLeft = false
let mutable moveRight = false

let mutable cursorVisible = true

let safeHideCursor () =
    if cursorVisible then
        Cursor.Hide()
        cursorVisible <- false

let safeShowCursor () =
    if not cursorVisible then
        Cursor.Show()
        cursorVisible <- true


let exitFPSMode () =
    isFPSMode <- false
    safeShowCursor()

let readCString (ptr: nativeint) : string =
    if ptr = nativeint 0 then ""
    else
        let mutable i = 0
        let buffer = ResizeArray<char>()
        let mutable b = Marshal.ReadByte(ptr, i)
        while b <> 0uy do
            buffer.Add(char b)
            i <- i + 1
            b <- Marshal.ReadByte(ptr, i)
        new String(buffer.ToArray())

let dumpNativeLogs (tag: string) =
    try
        let timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
        let filePath = $"native_log_{tag}_{timestamp}.txt"
        let logs = [
            $"=== NATIVE LOG DUMP: {tag} ==="
            "[LOG]"
            readCString (_getLog())
            "[WARNING]"
            readCString (_getWarningLog())
            "[ERROR]"
            readCString (_getErrorLog())
            $"Error Flag: {_getErrorFlag()}"
        ]
        File.WriteAllLines(filePath, logs)
        MessageBox.Show($"Logs written to {filePath}") |> ignore
    with ex ->
        MessageBox.Show("Failed to write logs: " + ex.Message) |> ignore

let ensureExists label path =
    if String.IsNullOrWhiteSpace(path) || not (File.Exists(path)) then
        _printError($"[F#] Missing or invalid path ({label}): {path}")
        failwithf "File does not exist or is invalid: %s" path

let getPictureBox () = pictureBox

let createViewerPanel () =
    pictureBox <- new PictureBox()
    pictureBox.Dock <- DockStyle.Fill
    pictureBox.BackColor <- Color.Black
    pictureBox.SizeMode <- PictureBoxSizeMode.Zoom
    pictureBox.TabStop <- true

    // Mouse Move (FPS mode)
    pictureBox.MouseMove.Add(fun _ ->
        if isFPSMode then
            let current = Cursor.Position
            let deltaX = current.X - screenCenter.X
            let deltaY = current.Y - screenCenter.Y

            if deltaX <> 0 || deltaY <> 0 then
                let sensitivity = 0.15f
                cameraYaw <- cameraYaw + float32 deltaX * sensitivity
                cameraPitch <- cameraPitch + float32 deltaY * sensitivity
                cameraPitch <- Math.Clamp(cameraPitch, -89.0f, 89.0f)

                _setCameraRotation(cameraPtr, cameraPitch, cameraYaw)
                appendLog $"FPS cam: Pitch={cameraPitch:F1} Yaw={cameraYaw:F1}"

                Cursor.Position <- screenCenter
    )

    // Mouse Click to enter FPS
    pictureBox.MouseClick.Add(fun args ->
        if args.Button = MouseButtons.Left then
            isFPSMode <- true
            screenCenter <- pictureBox.PointToScreen(Point(pictureBox.Width / 2, pictureBox.Height / 2))
            safeHideCursor()
            Cursor.Position <- screenCenter
            pictureBox.Focus() |> ignore
    )

    // Key Input
    pictureBox.KeyDown.Add(fun args ->
        match args.KeyCode with
        | Keys.W -> moveForward <- true
        | Keys.S -> moveBackward <- true
        | Keys.A -> moveLeft <- true
        | Keys.D -> moveRight <- true
        | Keys.Escape -> if isFPSMode then exitFPSMode()
        | _ -> ()
    )

    pictureBox.KeyUp.Add(fun args ->
        match args.KeyCode with
        | Keys.W -> moveForward <- false
        | Keys.S -> moveBackward <- false
        | Keys.A -> moveLeft <- false
        | Keys.D -> moveRight <- false
        | _ -> ()
    )

    pictureBox

let addToInspector name objPtr =
    let pos = _getGameObjectPosition(objPtr)
    let rot = _getGameObjectRotation(objPtr)
    let scale = _getGameObjectScale(objPtr)

    let objData = {
        Name = name
        Position = (pos.x, pos.y, pos.z)
        Rotation = (rot.x, rot.y, rot.z)
        Scale = (scale.x, scale.y, scale.z)
    }

    InspectorPanel.addObjectToInspector(objData, objPtr)

let initializeRenderer (_: int) (_: int) : unit =
    let width = pictureBox.Width
    let height = pictureBox.Height

    rendererPtr <- _createRenderer()
    if rendererPtr = nativeint 0 then failwith "Failed to create renderer."

    _setClearColor(rendererPtr, 0.2f, 0.2f, 0.2f)

    framePtr <- _createFrame(width, height)
    if framePtr = nativeint 0 then failwith "Failed to create frame."

    let aspectRatio = float32 width / float32 height
    cameraPtr <- _createCamera(75.0f, aspectRatio, 0.1f, 200.0f)
    if cameraPtr = nativeint 0 then failwith "Failed to create camera."

    _setCameraPosition(cameraPtr, -10.0f, 0.0f, 0.0f)

    // TEMP OBJECT
    let name  = "MUGMUG"
    let mesh  = @"C:\Users\aadha\Downloads\Mug Fbx and tex\Mug Fbx and tex\Mug.fbx"
    let diff  = @"C:\Users\aadha\Downloads\Mug Fbx and tex\Mug Fbx and tex\MugAlbedo.png"
    let norm  = @"C:\Users\aadha\Downloads\Mug Fbx and tex\Mug Fbx and tex\MugNormal.png"
    let metal = @"C:\Users\aadha\Downloads\Mug Fbx and tex\Mug Fbx and tex\MugMetalic.png"
    let rough = @"C:\Users\aadha\Downloads\Mug Fbx and tex\Mug Fbx and tex\MugRoughness.png"

    ensureExists "mesh" mesh
    ensureExists "diffuse" diff
    ensureExists "normal" norm
    ensureExists "metallic" metal
    ensureExists "roughness" rough

    let objPtr =
        try _createGameObject(name, mesh, diff, norm, metal, rough)
        with ex ->
            dumpNativeLogs("crash_createGameObject")
            raise ex

    gameObjectPtrs.Add(objPtr)
    let arr = gameObjectPtrs.ToArray()
    gameObjectGCHandle |> Option.iter (fun h -> h.Free()) // Free old handle
    gameObjectGCHandle <- Some(GCHandle.Alloc(arr, GCHandleType.Pinned))


    addToInspector name objPtr
    
let random = Random()

let randomRange (minVal: float32) (maxVal: float32) =
    minVal + (maxVal - minVal) * float32(random.NextDouble())

let createGameObjectFromFiles (name: string) (mesh: string) (diff: string) (norm: string) (metal: string) (rough: string) =
    ensureExists "mesh" mesh
    ensureExists "diffuse" diff
    ensureExists "normal" norm
    ensureExists "metallic" metal
    ensureExists "roughness" rough

    let objPtr =
        try _createGameObject(name, mesh, diff, norm, metal, rough)
        with ex ->
            raise ex

    // Randomize position, rotation, and scale
    let posX = randomRange -30.0f 30.0f
    let posY = randomRange -30.0f 30.0f
    let posZ = randomRange -30.0f 30.0f

    let rotX = randomRange 0.0f 180.0f
    let rotY = randomRange 0.0f 180.0f
    let rotZ = randomRange 0.0f 180.0f

    let scale = randomRange 0.5f 2.5f

    _moveGameObject(objPtr, posX, posY, posZ)
    _rotateGameObject(objPtr, rotX, rotY, rotZ)
    _scaleGameObject(objPtr, scale, scale, scale)

    gameObjectPtrs.Add(objPtr)
    let arr = gameObjectPtrs.ToArray()
    gameObjectGCHandle |> Option.iter (fun h -> h.Free()) // Free old handle
    gameObjectGCHandle <- Some(GCHandle.Alloc(arr, GCHandleType.Pinned))

    addToInspector name objPtr

let mutable lastWidth = -1
let mutable lastHeight = -1

let updateRender () =
    if rendererPtr = nativeint 0 || cameraPtr = nativeint 0 || framePtr = nativeint 0 then ()
    else
        let width = pictureBox.Width
        let height = pictureBox.Height

        if width <> lastWidth || height <> lastHeight then
            _updateFrameSize(framePtr, width, height)
            let aspect = float32 width / float32 height
            _setCameraAspect(cameraPtr, aspect)
            lastWidth <- width
            lastHeight <- height

        if isFPSMode then
            let speed = 0.2f
            let rot = _getCameraRotation(cameraPtr)
            let yaw = Math.PI * float rot.y / 180.0
            let pitch = Math.PI * float rot.x / 180.0

            let forward = Vector3(
                float32 (Math.Cos(pitch) * Math.Sin(yaw)),
                float32 (Math.Sin(pitch)),
                float32 (Math.Cos(pitch) * Math.Cos(yaw))
            )

            let right = Vector3(
                float32 (Math.Sin(yaw + Math.PI / 2.0)),
                0.0f,
                float32 (Math.Cos(yaw + Math.PI / 2.0))
            )

            let pos = _getCameraPosition(cameraPtr)
            let mutable newPos = Vector3(pos.x, pos.y, pos.z)

            if moveForward then newPos <- newPos + forward * speed
            if moveBackward then newPos <- newPos - forward * speed
            if moveRight then newPos <- newPos + right * speed
            if moveLeft then newPos <- newPos - right * speed

            _setCameraPosition(cameraPtr, newPos.X, newPos.Y, newPos.Z)

            cameraX <- newPos.X
            cameraY <- newPos.Y
            cameraZ <- newPos.Z

        let ptrToArray, count =
            match gameObjectGCHandle with
            | Some handle when gameObjectPtrs.Count > 0 -> handle.AddrOfPinnedObject(), gameObjectPtrs.Count
            | _ -> nativeint 0, 0

        let pixelData = _renderFrame(rendererPtr, ptrToArray, count, cameraPtr, framePtr)


        let pos = _getCameraPosition(cameraPtr)
        appendLog(sprintf "Camera Position: X=%.2f, Y=%.2f, Z=%.2f" pos.x pos.y pos.z)

        if pixelData.p <> nativeint 0 && pixelData.count > 0 then
            try
                let expectedRgbSize = width * height * 3
                if pixelData.count = expectedRgbSize then
                    let rgbBytes = Array.zeroCreate<byte> pixelData.count
                    Marshal.Copy(pixelData.p, rgbBytes, 0, pixelData.count)

                    let rgbaBytes = Array.zeroCreate<byte> (width * height * 4)
                    for i in 0 .. (width * height - 1) do
                        rgbaBytes[i * 4 + 0] <- rgbBytes[i * 3 + 0]
                        rgbaBytes[i * 4 + 1] <- rgbBytes[i * 3 + 1]
                        rgbaBytes[i * 4 + 2] <- rgbBytes[i * 3 + 2]
                        rgbaBytes[i * 4 + 3] <- 255uy

                    let bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb)
                    let data = bmp.LockBits(Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
                    Marshal.Copy(rgbaBytes, 0, data.Scan0, rgbaBytes.Length)
                    bmp.UnlockBits(data)

                    if not (isNull pictureBox.Image) then
                        pictureBox.Image.Dispose()

                    pictureBox.Image <- bmp
            with ex ->
                MessageBox.Show("UpdateRender failed: " + ex.Message) |> ignore
