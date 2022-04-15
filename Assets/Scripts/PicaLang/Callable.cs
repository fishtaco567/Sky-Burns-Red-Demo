using System.Collections.Generic;

namespace Pica {

    public interface Callable {

        object Call(PicaInterpreter interpreter, List<object> args);

        int Arity();

    }

}