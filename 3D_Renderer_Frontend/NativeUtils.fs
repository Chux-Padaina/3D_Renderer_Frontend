module NativeUtils

open System
open System.Runtime.InteropServices
open System.Text

let ptrToString (ptr: nativeint) : string =
    if ptr = nativeint 0 then
        ""
    else
        Marshal.PtrToStringAnsi(ptr)
