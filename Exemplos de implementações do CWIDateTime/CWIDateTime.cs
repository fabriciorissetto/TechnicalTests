namespace BeltranoDosSantos
{
    public class CWIDateTime
    {
        public string ChangeDate(string date, char operation, long value)
        {
            var dt = date.Split(' ')[0];
            var tempo = date.Split(' ')[1];
            long d = int.Parse(dt.Split('/')[0]);
            long ms = int.Parse(dt.Split('/')[1]);
            long opera = int.Parse(dt.Split('/')[2]);
            long pos = int.Parse(tempo.Split(':')[0]);
            long mnt = int.Parse(tempo.Split(':')[1]);
            
            if (value < 0) value *= -1;
            long t = Verifica(d, ms, opera, pos, mnt);
            
            if (operation == '+')
            {
                t += value;
            }
            else if (operation == '-')
            {
                t -= value;
            }
            return Conv(t);
        }

        private static string Conv(long tm)
        {
            var ano = tm / 525600;
            var ms = tm % 525600;

            long mss = 0;
            for (var i = 1; i <= 12; i++)
            {
                if (GetTot(i) > ms)
                {
                    mss = i - 1;
                    ms = ms - GetTot(i - 1);
                    break;
                }
            }
            var dd = ms / 1440;
            ms = ms % 1440;
            var dividido = ms / 60;
            ms = ms % 60;


            return dd.ToString().PadLeft(2, '0') + "/" + mss.ToString().PadLeft(2, '0') + "/" + ano.ToString().PadLeft(4, '0') + " " +
                   dividido.ToString().PadLeft(2, '0') + ":" + ms.ToString().PadLeft(2, '0');
        }
        
        private static long Verifica(long dias, long meses, long anos, long horas, long minutos)
        {
            var tmt = anos * 525600;

            var msk = GetTot(meses);

            tmt += msk + dias * 1440 + horas * 60 + minutos;

            return tmt;
        }


        private static long GetTot(long o)
        {
            long m = 0;

            switch (o)
            {
                case 1:
                    m = 0;
                    break;
                case 2:
                    m = 44640;
                    break;
                case 3:
                    m = 44640 + 40320;
                    break;
                case 4:
                    m = 44640 * 2 + 40320;
                    break;
                case 5:
                    m = 44640 * 2 + 43200 + 40320;
                    break;
                case 6:
                    m = 44640 * 3 + 43200 + 40320;
                    break;
                case 7:
                    m = 44640 * 3 + 43200 * 2 + 40320;
                    break;
                case 8:
                    m = 44640 * 4 + 43200 * 2 + 40320;
                    break;
                case 9:
                    m = 44640 * 5 + 43200 * 2 + 40320;
                    break;
                case 10:
                    m = 44640 * 5 + 43200 * 3 + 40320;
                    break;
                case 11:
                    m = 44640 * 6 + 43200 * 3 + 40320;
                    break;
                case 12:
                    m = 44640 * 6 + 43200 * 4 + 40320;
                    break;
            }

            return m;
        }
    }
}
