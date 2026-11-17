using System;
using System.Runtime.InteropServices;
public class Glass {
 [DllImport("user32.dll",EntryPoint="GetWindowLongW")] static extern int GetWindowLong(IntPtr hwnd,int index);
 [DllImport("user32.dll",EntryPoint="SetWindowLongW")] static extern int SetWindowLong(IntPtr hwnd,int index,int value);
 public static bool IsToolWindow(IntPtr hwnd){return (GetWindowLong(hwnd,-20)&0x80)!=0;}
 [StructLayout(LayoutKind.Sequential)] struct Accent { public int State,Flags,Color,Animation; }
 [StructLayout(LayoutKind.Sequential)] struct Data { public int Attribute; public IntPtr Pointer; public int Size; }
 [DllImport("user32.dll")] static extern int SetWindowCompositionAttribute(IntPtr hwnd,ref Data data);
 [StructLayout(LayoutKind.Sequential)] struct Margins { public int L,R,T,B; }
 [DllImport("dwmapi.dll")] static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd,ref Margins margins);
 [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd,int attr,ref int value,int size);
 [StructLayout(LayoutKind.Sequential)] struct Point { public int X,Y; }
 [StructLayout(LayoutKind.Sequential)] struct Rect { public int L,T,R,B; }
 [DllImport("user32.dll")] static extern bool GetCursorPos(out Point point);
 [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
 [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hwnd,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("gdi32.dll")] static extern IntPtr CreateRoundRectRgn(int left,int top,int right,int bottom,int ellipseWidth,int ellipseHeight);
 [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
 [DllImport("user32.dll")] static extern int SetWindowRgn(IntPtr hwnd,IntPtr region,bool redraw);
 [DllImport("user32.dll")] static extern int GetWindowRgn(IntPtr hwnd,IntPtr region);
 [DllImport("gdi32.dll")] static extern bool PtInRegion(IntPtr region,int x,int y);
 static int[] dragOrigin;
 static double logicalWidth;
 delegate IntPtr SubclassProc(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp,UIntPtr id,IntPtr data);
 static readonly SubclassProc dragProc=OnMessage;
 [DllImport("comctl32.dll")] static extern bool SetWindowSubclass(IntPtr hwnd,SubclassProc callback,UIntPtr id,IntPtr data);
 [DllImport("comctl32.dll")] static extern IntPtr DefSubclassProc(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp);
 [DllImport("user32.dll")] static extern IntPtr SetCapture(IntPtr hwnd);
 [DllImport("user32.dll")] static extern bool ReleaseCapture();
 public static void Attach(IntPtr hwnd,double width) {
  logicalWidth=width;
  if(!SetWindowSubclass(hwnd,dragProc,new UIntPtr(1),IntPtr.Zero))throw new Exception("Cannot attach native drag handler");
 }
 public static void StartDrag(IntPtr hwnd) {
  dragOrigin=BeginDrag(hwnd);
  if(dragOrigin==null)return;
  SetCapture(hwnd);
 }
 static void EndDrag(IntPtr hwnd) {
  if(dragOrigin==null)return;
  MoveDrag(hwnd,dragOrigin);dragOrigin=null;
  ReleaseCapture();Clip(hwnd,logicalWidth);
 }
 static IntPtr OnMessage(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp,UIntPtr id,IntPtr data) {
  if(dragOrigin!=null){
   if(msg==0x200){
    if((wp.ToInt64()&1)!=0)MoveDrag(hwnd,dragOrigin);else EndDrag(hwnd);
    return IntPtr.Zero;
   }
   if(msg==0x202||msg==0x215||msg==0x1F){EndDrag(hwnd);return IntPtr.Zero;}
  }
  return DefSubclassProc(hwnd,msg,wp,lp);
 }
 public static int[] WindowBounds(IntPtr hwnd) {Rect r;GetWindowRect(hwnd,out r);return new int[]{r.L,r.T,r.R,r.B};}
 public static int[] BeginDrag(IntPtr hwnd) {
  Point p; Rect r;
  if(!GetCursorPos(out p)||!GetWindowRect(hwnd,out r))return null;
  return new int[]{p.X,p.Y,r.L,r.T};
 }
 public static int[] DragPosition(int[] origin,int x,int y) {
  return new int[]{origin[2]+x-origin[0],origin[3]+y-origin[1]};
 }
 public static void MoveDrag(IntPtr hwnd,int[] origin) {
  Point p;if(!GetCursorPos(out p))return;
  var pos=DragPosition(origin,p.X,p.Y);
  SetWindowPos(hwnd,IntPtr.Zero,pos[0],pos[1],0,0,0x0015);
 }
 // A layered window already has per-pixel alpha. A GDI region would replace
 // the smooth edge with a binary, stair-stepped boundary.
 public static void Clip(IntPtr hwnd,double logicalWidth) {SetWindowRgn(hwnd,IntPtr.Zero,true);}
 public static bool VerifySoftBoundary(IntPtr hwnd) {
  var region=CreateRoundRectRgn(0,0,1,1,1,1);
  try{return GetWindowRgn(hwnd,region)==0;}finally{DeleteObject(region);}
 }
 public static void Apply(IntPtr hwnd) {
  SetWindowLong(hwnd,-20,(GetWindowLong(hwnd,-20)|0x80)&~0x40000);
  // Layered alpha defines the silhouette; do not extend a rectangular glass frame.
  int corners=1; DwmSetWindowAttribute(hwnd,33,ref corners,4);
  SetAccent(hwnd,0);
 }
 public static void SetAccent(IntPtr hwnd,int state) {
  var accent=new Accent { State=state, Flags=0, Color=unchecked((int)0x18302821) };
  var ptr=Marshal.AllocHGlobal(Marshal.SizeOf(accent));
  try { Marshal.StructureToPtr(accent,ptr,false); var data=new Data {Attribute=19,Pointer=ptr,Size=Marshal.SizeOf(accent)}; SetWindowCompositionAttribute(hwnd,ref data); }
  finally { Marshal.FreeHGlobal(ptr); }
 }
}
