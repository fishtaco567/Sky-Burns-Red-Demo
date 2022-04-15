using System.Collections.Generic;
using System;
using UnityEngine;

namespace Pica {

    public class PicaRuntime {

        public Dictionary<string, List<PicaInterpreter>> programSpaces;

        public PicaEnv ffi;

        public PicaRuntime() {
            programSpaces = new Dictionary<string, List<PicaInterpreter>>();
            SetupFFI();
        }

        public void SetupFFI() {
            ffi = new PicaEnv();
            GameEventInterface.Setup(ffi);
            BuiltInFunctions.Setup(ffi);
        }

        public void UpdateFFI() {
            ffi.Set("time", (double)Time.time);
            ffi.Set("dt", (double)Time.deltaTime);
        }

        public void RunTick() {
            UpdateFFI();

            foreach(var space in programSpaces.Values) {
                if(space.Count == 0) {
                    continue;
                }

                for(int i = space.Count - 1; i >= 0; i--) {
                    if(space[i].Run()) {
                        space.RemoveAt(i);
                    }
                }
            }
        }

        public void ClearSpace(string name) {
            programSpaces[name] = new List<PicaInterpreter>();
        }

        public void AddProgram(string space, string program) {
            var tokens = new PicaLexer(program).Lex();
            var statements = new PicaParser(tokens).Parse();
            var interpreter = new PicaInterpreter(statements, ffi);

            AddProgram(space, interpreter);
        }

        public void AddProgram(string space, PicaInterpreter interpreter) {
            if(programSpaces.ContainsKey(space)) {
                programSpaces[space].Add(interpreter);
            } else {
                programSpaces[space] = new List<PicaInterpreter>();
                programSpaces[space].Add(interpreter);
            }
        }

    }

}