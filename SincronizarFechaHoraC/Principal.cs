using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SincronizarFechaHoraC
{
    class Principal
    {
        static void Main(string[] args)
        {
            SincronizarFechaHora sincronizarFechaHora = new SincronizarFechaHora();

            sincronizarFechaHora.Sincronizar();

            //Console.ReadKey();
        }
    }
}
