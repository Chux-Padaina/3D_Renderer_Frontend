module NativeBindings

open System
open System.Runtime.InteropServices

// ===== Native Structs =====
[<StructLayout(LayoutKind.Sequential)>]
type Vector3Export =
    struct
        val mutable x: float32
        val mutable y: float32
        val mutable z: float32
    end
[<StructLayout(LayoutKind.Sequential)>]
type PixelData =
    struct
        val mutable p: nativeint
        val mutable count: int
    end

// ===== Logs =====
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint _getLog()

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint _getWarningLog()

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint _getErrorLog()

// ===== Renderer Setup =====
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint _createRenderer()

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _setClearColor(nativeint renderer, float32 r, float32 g, float32 b)

// ===== Camera Setup =====
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint _createCamera(float32 fov, float32 aspect, float32 near, float32 far)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _setCameraPosition(nativeint camera, float32 x, float32 y, float32 z)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _setCameraAspect(nativeint camera, float32 aspect)

// ===== Framebuffer =====
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern nativeint _createFrame(int width, int height)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _updateFrameSize(nativeint frame, int width, int height)

// ===== Rendering =====
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern PixelData _renderFrame(nativeint renderer, nativeint gameObjects, int count, nativeint camera, nativeint frame)


// =======LOGS=====
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _print(string message)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _printWarning(string message)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _printError(string message)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern int _getErrorFlag()

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
extern nativeint _createGameObject(
    string name,
    string meshPath,
    string diffusePath,
    string normalPath,
    string metallicPath,
    string roughnessPath
)

// Camera Controls
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _setCameraRotation(nativeint camera, float32 pitch, float32 yaw)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern Vector3Export _getCameraPosition(nativeint camera)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern Vector3Export _getCameraRotation(nativeint camera)


// OBJ controls
[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _moveGameObject(nativeint objPtr, float32 x, float32 y, float32 z)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _rotateGameObject(nativeint objPtr, float32 x, float32 y, float32 z)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void _scaleGameObject(nativeint objPtr, float32 x, float32 y, float32 z)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern Vector3Export _getGameObjectPosition(nativeint objPtr)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern Vector3Export _getGameObjectRotation(nativeint objPtr)

[<DllImport("OpenGLRenderEngine.dll", CallingConvention = CallingConvention.Cdecl)>]
extern Vector3Export _getGameObjectScale(nativeint objPtr)