using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SistemasOperacionais.Interfaces;
using SistemasOperacionais.Modelos;

namespace SistemasOperacionais.Maquina
{
    public class CPU
    {
        private readonly IEscalonador _escalonador;
        private readonly Memoria _memoria;

        public CPU(IEscalonador escalonador, Memoria memoria)
        {
            _escalonador = escalonador ?? throw new ArgumentNullException(nameof(escalonador));
            _memoria = memoria ?? throw new ArgumentNullException(nameof(memoria));
        }

        /// <summary>
        /// Roda uma itera��o de execu��o: pega da mem�ria processos prontos, escolhe pelo escalonador e executa.
        /// Aqui usamos simula��o com Task.Delay para representar o tempo de execu��o (TempoExecucao em ms).
        /// </summary>
        public async Task RodarAsync(int ciclo)
        {
            Console.WriteLine($"\n=== CICLO {ciclo} ===");

            var prontos = _memoria.ProcessosNaMemoria
                .Where(p => p.Estado == EstadoProcesso.Pronto)
                .ToList();

            var proximo = _escalonador.DecidirProximoProcesso(prontos);
            if (proximo == null)
            {
                Console.WriteLine("CPU: Nenhum processo pronto para executar.");
                return;
            }

            Console.WriteLine($"CPU: Escalonado processo PID {proximo.Id} ({proximo.GetType().Name})");
            proximo.Executar();

            await Task.Delay(Math.Max(0, proximo.TempoExecucao));

            proximo.Finalizar();
            _memoria.LiberarMemoria(proximo);
            Console.WriteLine($"CPU: Processo PID {proximo.Id} finalizado e mem�ria liberada.");

            // Mostra estado atual da mem�ria
            _memoria.MostrarEstadoMemoria();

            Console.WriteLine($"=== FIM CICLO {ciclo} ===");
            await Task.Delay(1000); // pausa 1s antes do pr�ximo ciclo
        }


        /// <summary>
        /// Executa v�rias itera��es at� n�o sobrar processos na mem�ria.
        /// </summary>
        public async Task RodarTudoAsync()
        {
            int ciclo = 1;
            while (_memoria.ProcessosNaMemoria.Any(p => p.Estado == EstadoProcesso.Pronto))
            {
                await RodarAsync(ciclo++);
            }

            Console.WriteLine("CPU: Todos processos finalizados ou nenhum pronto restante.");
        }
    }
}
