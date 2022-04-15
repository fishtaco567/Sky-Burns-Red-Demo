using UnityEngine;
using System;

namespace Pica {

    public class PicaError {

        public static void Error(string error, int line) {
            ShowError(error, "", line);
        }

        static void ShowError(string error, string place, int line) {
            string message = "Line " + line + " error" + place + ": " + error;

            Debug.LogError(message);
        }

    }

    public class RuntimeError : Exception {

        public Token op;
        public string message;

        public RuntimeError(Token op, string message) {
            this.op = op;
            this.message = message;
        }

    }

}