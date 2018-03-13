using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKSoftwareAPI;
using Npgsql;
using System.Configuration;

namespace SincronizarFechaHoraC
{
    class SincronizarFechaHora
    {
        ZKSoftware dispositivo;

        Int32 biometrico_id;

        string parametros = ConfigurationManager.ConnectionStrings["tritonPgsql"].ConnectionString;

        NpgsqlConnection conexion1 = new NpgsqlConnection();
        NpgsqlConnection conexion4 = new NpgsqlConnection();

        public SincronizarFechaHora()
        {
            dispositivo = new ZKSoftware(Modelo.X628C);
        }

        public void Sincronizar()
        {
            bool conexion_db_1 = ConexionDB(conexion1);

            if (conexion_db_1)
            {
                string sql1 = "SELECT id, ip FROM rrhh_biometricos WHERE estado=4;";
                NpgsqlCommand comando1 = new NpgsqlCommand(sql1, conexion1);
                NpgsqlDataReader consulta1 = comando1.ExecuteReader();
                while (consulta1.Read())
                {
                    biometrico_id = Convert.ToInt32(consulta1[0]);
                    string ip = Convert.ToString(consulta1[1]);

                    bool conexion_sw = Conectar(ip, 0, false);

                    if (conexion_sw)
                    {
                        if (!dispositivo.DispositivoCambiarHoraAutomatico())
                        {
                            Console.WriteLine(dispositivo.ERROR);
                        }
                        Desconectar();
                    }
                }
                CerrarConexionBD(conexion1);
            }
        }

        private bool Conectar(String ip, int intentos, bool alerta)
        {
            if (!dispositivo.DispositivoConectar(ip, intentos, alerta))
            {
                Console.WriteLine(dispositivo.ERROR);
                bool conexion_db_4 = ConexionDB(conexion4);
                if (conexion_db_4)
                {
                    string sql = "INSERT INTO rrhh_log_alertas (biometrico_id, tipo_emisor, tipo_alerta, f_alerta, mensaje) VALUES(" + biometrico_id + ", 1, 1, '" + DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss") + "', '" + dispositivo.ERROR + "');";
                    sql += " UPDATE rrhh_biometricos SET e_conexion=2, fs_conexion='" + DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss") + "' WHERE id=" + biometrico_id + ";";
                    NpgsqlCommand comando4 = conexion4.CreateCommand();
                    comando4.CommandText = sql;
                    comando4.ExecuteNonQuery();

                    CerrarConexionBD(conexion4);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Desconectar()
        {
            dispositivo.DispositivoDesconectar();
        }

        //=== BASE DE DATOS ===
        private bool ConexionDB(NpgsqlConnection conexion)
        {
            conexion.ConnectionString = parametros;

            try
            {
                conexion.Open();
                return true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return false;
            }
        }

        private void CerrarConexionBD(NpgsqlConnection conexion)
        {
            conexion.Close();
        }
    }
}
