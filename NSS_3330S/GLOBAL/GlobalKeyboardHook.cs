using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DionesTool.UTIL 
{
	/// <summary>
	/// A class that manages a global low level keyboard hook
	/// </summary>
    /// 
    //
    //    GlobalKeyboardHook ghKeyboard = new GlobalKeyboardHook();
    //    private bool _bShiftPressed = false;
    /*
            ghKeyboard.HookedKeys.Add(Keys.Left);
            ghKeyboard.HookedKeys.Add(Keys.Right);
            ghKeyboard.HookedKeys.Add(Keys.Up);
            ghKeyboard.HookedKeys.Add(Keys.Down);
            ghKeyboard.HookedKeys.Add(Keys.LShiftKey);
            ghKeyboard.HookedKeys.Add(Keys.RShiftKey);
            ghKeyboard.KeyDown += new KeyEventHandler(gkh_KeyDown);
            ghKeyboard.KeyUp += new KeyEventHandler(gkh_KeyUp);
     */
    /*
     void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.LShiftKey) || (e.KeyCode == Keys.RShiftKey))
            {
                _bShiftPressed = false;
                e.Handled = true;
            }

            e.Handled = true;
        }

        void gkh_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.LShiftKey) || (e.KeyCode == Keys.RShiftKey))
            {
                _bShiftPressed = true;
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Left)
            {
                if (_bShiftPressed == true)
                {
                    //lstLog.Items.Add("Shift\t" + e.KeyCode.ToString());
                }
                else
                {                    
                    //lstLog.Items.Add("Down\t" + e.KeyCode.ToString());
                }
            }

            e.Handled = true;
        }
     */
    public class GlobalKeyboardHook 
    {
		#region Constant, Structure and Delegate Definitions
		/// <summary>
		/// defines the callback type for the hook
		/// </summary>

        keyboardHookProc m_hookProc = null;
		public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

		public struct keyboardHookStruct 
        {
			public int vkCode;
			public int scanCode;
			public int flags;
			public int time;
			public int dwExtraInfo;
		}

		const int WH_KEYBOARD_LL = 13;
		const int WM_KEYDOWN = 0x100;
		const int WM_KEYUP = 0x101;
		const int WM_SYSKEYDOWN = 0x104;
		const int WM_SYSKEYUP = 0x105;
		#endregion

		#region Instance Variables
		/// <summary>
		/// The collections of keys to watch for
		/// </summary>
		public List<Keys> HookedKeys = new List<Keys>();
		/// <summary>
		/// Handle to the hook, need this to unhook and call the next hook
		/// </summary>
		IntPtr hhook = IntPtr.Zero;
		#endregion

		#region Events
		/// <summary>
		/// Occurs when one of the hooked keys is pressed
		/// </summary>
		public event KeyEventHandler KeyDown;
		/// <summary>
		/// Occurs when one of the hooked keys is released
		/// </summary>
		public event KeyEventHandler KeyUp;
		#endregion

		#region Constructors and Destructors
		/// <summary>
		/// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
		/// </summary>
		public GlobalKeyboardHook() 
        {
			hook();
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="globalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
		/// </summary>
        ~GlobalKeyboardHook() 
        {
			unhook();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Installs the global hook
		/// </summary>
		public void hook() 
        {
			IntPtr hInstance = LoadLibrary("User32");
            m_hookProc = hookProc;

            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, m_hookProc, hInstance, 0);
		}

		/// <summary>
		/// Uninstalls the global hook
		/// </summary>
		public void unhook() 
        {
			UnhookWindowsHookEx(hhook);
		}

		/// <summary>
		/// The callback for the keyboard hook
		/// </summary>
		/// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
		/// <param name="wParam">The event type</param>
		/// <param name="lParam">The keyhook event information</param>
		/// <returns></returns>
		public int hookProc(int code, int wParam, ref keyboardHookStruct lParam) 
        {
            int nRet;
			if (code >= 0) 
            {
				Keys key = (Keys)lParam.vkCode;
				if (HookedKeys.Contains(key)) 
                {
					KeyEventArgs kea = new KeyEventArgs(key);
					if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null)) 
                    {
						KeyDown(this, kea) ;
					} 
                    else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null)) 
                    {
						KeyUp(this, kea);
					}
					if (kea.Handled)
						return 1;
				}
			}
			nRet = CallNextHookEx(hhook, code, wParam, ref lParam);
            return nRet;
		}
		#endregion

		#region DLL imports
		/// <summary>
		/// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
		/// </summary>
		/// <param name="idHook">The id of the event you want to hook</param>
		/// <param name="callback">The callback.</param>
		/// <param name="hInstance">The handle you want to attach the event to, can be null</param>
		/// <param name="threadId">The thread you want to attach the event to, can be null</param>
		/// <returns>a handle to the desired hook</returns>
		[DllImport("user32.dll")]
		static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

		/// <summary>
		/// Unhooks the windows hook.
		/// </summary>
		/// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
		/// <returns>True if successful, false otherwise</returns>
		[DllImport("user32.dll")]
		static extern bool UnhookWindowsHookEx(IntPtr hInstance);

		/// <summary>
		/// Calls the next hook.
		/// </summary>
		/// <param name="idHook">The hook id</param>
		/// <param name="nCode">The hook code</param>
		/// <param name="wParam">The wparam.</param>
		/// <param name="lParam">The lparam.</param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

		/// <summary>
		/// Loads the library.
		/// </summary>
		/// <param name="lpFileName">Name of the library</param>
		/// <returns>A handle to the library</returns>
		[DllImport("kernel32.dll")]
		static extern IntPtr LoadLibrary(string lpFileName);
		#endregion
	}
}
