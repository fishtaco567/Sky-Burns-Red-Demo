public class SkyEventStringReader {

    public string[] lines;
    public int position;

    public SkyEventStringReader(string s) {
        lines = s.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        position = 0;
    }

    public string ReadLine() {
        if(position >= lines.Length) {
            return null;
        }

        var s = lines[position];
        position++;
        return s;
    }

    public bool HasNext() {
        return position < lines.Length;
    }

    public string PeekLine() {
        if(position >= lines.Length) {
            return null;
        }

        return lines[position];
    }

    public void MoveHead(int amount) {
        position += amount;
        if(position < 0) {
            position = 0;
        }
    }

}