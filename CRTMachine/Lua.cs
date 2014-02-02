using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicLua;
using LuaInterface;
using LuaNative;
using System.Reflection;
using System.Collections;

namespace CRTMachine {
    internal static class Lua {
        static DynamicLua.DynamicLua _Lua =/* new DynamicLua.DynamicLua(true);*/ new DynamicLua.DynamicLua();
        internal static dynamic _DLua = _Lua;
        static IntPtr _State;

        internal delegate object LFunc(LuaInterface.LuaTable Arg);

        internal static void Initialize() {
            /*_State = _Lua.LuaInstance.luaState;*/
            _State = new IntPtr(0);
            _Lua.NewTable("__VARG");
            RegisterTable("system");
            //_Lua.LuaInstance.InitMscorlib();

        }

        internal static void RegisterTable(string Name) {
            _Lua.NewTable(Name);
            _Lua.NewTable("__VARG." + Name);
        }

        internal static void LoadLibs(CRT.System Sys) {
            Register("system.print", (A) => {
                Sys.print(A[1].ToString());
                return null;
            });
            Register("system.clear", (A) => {
                Sys.clear();
                return null;
            });
            Register("system.putchr", (A) => {
                Sys.putchr(A[1].ToString()[0]);
                return null;
            });
            Register("system.read", (A) => {
                return Sys.read().ToString();
            });
            Register("system.readkey", (A) => {
                return Sys.readkey();
            });
        }

        internal static void DoFile(string Path) {
            _Lua.DoFile(Path.Replace('/', '\\'));
        }

        internal static void DoString(string S) {
            try {
                _Lua.LuaInstance.DoString(S);
            } catch (Exception E) {
                Console.WriteLine(E.Message);
            }
        }

        internal static void Register(string Name, LFunc F) {
            _DLua.__VARG[Name] = F;
            _Lua.LuaInstance.DoString(Name + " = function(...) return __VARG." + Name + "({...}); end");
        }

        internal static void Remove(string S) {
            /*LuaDLL.lua_pushnil(_State);
            LuaDLL.lua_setglobal(_State, S);*/
        }

        internal static void Remove(params string[] S) {
            for (int i = 0; i < S.Length; i++)
                Remove(S[i]);
        }
    }
}