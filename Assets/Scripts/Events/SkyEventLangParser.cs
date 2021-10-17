using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyEventLangParser {

    public SkyEventProgram Parse(string source) {
        var programList = new List<SkyEventExpression>();

        var reader = new SkyEventStringReader(source);
        while(reader.HasNext()) {
            programList.Add(ParseInstruction(reader));
        }

        var program = new SkyEventProgram(programList);
        return program;
    }

    public SkyEventExpression ParseInstruction(SkyEventStringReader reader) {
        var line = reader.ReadLine();

        if(line.Length != 0) {
            //Check for reserved words
            var splitstring = line.Trim().Split(new char[] { ' ' }, 2);
            if(splitstring.Length >= 1) {
                switch(splitstring[0].Trim()) {
                    case "WaitUntil":
                        Log("Adding Wait Until with args: " + splitstring[1]);

                        if(splitstring.Length == 2) {
                            return new WaitUntilExpression(splitstring[1]);
                        } else {
                            LogError("ParamsError");
                        }
                        break;
                    case "WaitSeconds":
                        Log("Adding Wait Seconds with args: " + splitstring[1]);

                        if(splitstring.Length == 2) {
                            return new WaitSecondsExpression(splitstring[1]);
                        } else {
                            LogError("ParamsError");
                        }
                        break;
                    case "DispatchEventWithDelay":
                        Log("Adding DispatchEventWithDelay with args: " + splitstring[1]);

                        if(splitstring.Length == 2) {
                            return new DispatchEventWithDelayExpression(splitstring[1]);
                        } else {
                            LogError("ParamsError");
                        }
                        break;
                    case "DispatchEventNow":
                        Log("Adding DispatchEventNow with args: " + splitstring[1]);

                        if(splitstring.Length == 2) {
                            return new DispatchEventNowExpression(splitstring[1]);
                        } else {
                            LogError("ParamsError");
                        }
                        break;
                    case "LoopWhile":
                        Log("Adding LoopWhile with args: " + splitstring[1]);

                        if(splitstring.Length == 2) {
                            return ParseWhileLoopStructure(reader, splitstring[1], IndentLevel(line) + 1);
                        } else {
                            LogError("ParamsError");
                        }
                        break;
                    case "LoopFor":
                        Log("Adding LoopFor with args: " + splitstring[1]);
                        if(splitstring.Length == 2) {
                            return ParseForLoopStructure(reader, splitstring[1], IndentLevel(line) + 1);
                        } else {
                            LogError("ParamsError");
                        }
                        break;
                    case "If":
                        Log("Adding If with args: " + splitstring[1]);

                        if(splitstring.Length == 2) {
                            return ParseIfElseStructure(reader, splitstring[1], IndentLevel(line) + 1);
                        } else {
                            LogError("ParamsError");
                        }
                        break;
                    case "ElseIf":
                        LogError("SyntaxError, ElseIf can only occur following If");
                        break;
                    case "Else":
                        LogError("SyntaxError, Else can only occur following If");
                        break;
                    default:
                        //Check for variable assignment
                        if(line.Contains("=")) {
                            Log("Adding variable expression: " + line);
                            return new VariableExpression(line);
                        }
                        break;
                }
            }
        }

        return null;
    }

    public SkyEventExpression ParseIfElseStructure(SkyEventStringReader reader, string firstArg, int indentLevel) {
        var currentArg = firstArg;
        var currentInstructions = new List<SkyEventExpression>();

        var ifExp = new IfElseExpression();

        var atElse = false;

        while(reader.HasNext()) {
            var linePeek = reader.PeekLine();
            if(linePeek.Length != 0) {
                if(IndentLevel(linePeek) >= indentLevel) {
                    currentInstructions.Add(ParseInstruction(reader));
                } else {
                    if(atElse == true) {
                        break;
                    }

                    var splitstring = linePeek.Trim().Split(new char[] { ' ' }, 2);
                    if(splitstring[0].Trim() == "ElseIf") {
                        Log("Adding IfElse with args: " + splitstring[1]);

                        ifExp.AddIfElse(currentArg, currentInstructions);
                        reader.MoveHead(1);

                        currentArg = splitstring[1];
                        currentInstructions = new List<SkyEventExpression>();
                    } else if(splitstring[0].Trim() == "Else") {
                        Log("Adding Else");

                        atElse = true;

                        ifExp.AddIfElse(currentArg, currentInstructions);
                        reader.MoveHead(1);

                        currentArg = "true";
                        currentInstructions = new List<SkyEventExpression>();
                    } else {
                        break;
                    }
                }
            } else {
                reader.MoveHead(1);
            }
        }
        ifExp.AddIfElse(currentArg, currentInstructions);

        return ifExp;
    }
    public SkyEventExpression ParseForLoopStructure(SkyEventStringReader reader, string args, int indentLevel) {
        var currentInstructions = new List<SkyEventExpression>();

        while(reader.HasNext()) {
            var linePeek = reader.PeekLine();
            if(linePeek.Length != 0) {
                if(IndentLevel(linePeek) >= indentLevel) {
                    currentInstructions.Add(ParseInstruction(reader));
                } else {
                    break;
                }
            } else {
                reader.MoveHead(1);
            }
        }

        return new LoopForExpression(args, currentInstructions);
    }

    public SkyEventExpression ParseWhileLoopStructure(SkyEventStringReader reader, string args, int indentLevel) {
        var currentInstructions = new List<SkyEventExpression>();

        while(reader.HasNext()) {
            var linePeek = reader.PeekLine();
            if(linePeek.Length != 0) {
                if(IndentLevel(linePeek) >= indentLevel) {
                    currentInstructions.Add(ParseInstruction(reader));
                } else {
                    break;
                }
            } else {
                reader.MoveHead(1);
            }
        }

        return new LoopWhileExpression(args, currentInstructions);
    }

    public int IndentLevel(string s) {
        int indentLevel = 0;
        int spaceCount = 0;
        for(int i = 0; i < s.Length; i++) {
            switch(s[i]) {
                case '\t':
                    indentLevel++;
                    spaceCount = 0;
                    break;
                case ' ':
                    spaceCount++;
                    if(spaceCount >= 4) {
                        indentLevel++;
                        spaceCount = 0;
                    }
                    break;
                default:
                    return indentLevel;
            }
        }

        return indentLevel;
    }

    private void Log(string s) {
        Debug.Log(s);
    }

    private void LogError(string s) {
        Debug.LogError(s);
    }

}
