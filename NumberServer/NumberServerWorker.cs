using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NumberServer
{
    public class NumberServerWorker
    {
        enum Precedence
        {
            First,
            Second
        }

        private string _input;
        private string[] _inputArray;
        private int _arrayCounter;
        private int _arrayLengthCounter;

        public void Start()
        {
            TcpListener socket = new TcpListener(IPAddress.Loopback, 3001);
            socket.Start();


            while (true)
            {
                TcpClient client = socket.AcceptTcpClient();

                Task.Run(() => DoClient(client));
            }
        }

        private void DoClient(TcpClient client)
        {
            while (true)
            {
                NetworkStream ns = client.GetStream();

                StreamReader reader = new StreamReader(ns);
                StreamWriter writer = new StreamWriter(ns);

                _input = reader.ReadLine();
                _inputArray = _input?.Split(new[] {'+', '-', '*', '/'});
                if (_input == null || _inputArray[0] == "STOP")
                {
                    writer.WriteLine("Disconnecting");
                    writer.Flush();
                    client.Close();
                    break;
                }

                writer.WriteLine(Calculate(GetPrecedence(_input[_inputArray[0].Length])));

                writer.Flush();

                Clean();
            }
        }

        private double Calculate(Precedence precedence)
        {
            double sum = Convert.ToDouble(_inputArray[_arrayCounter]);

            while (_arrayCounter < _inputArray.Length-1)
            {
                _arrayLengthCounter += _inputArray[_arrayCounter].Length+1;

                Precedence decidingPrecedence;
                if (_arrayCounter < _inputArray.Length-2)
                {
                    int decidingSymbolIndex = _arrayLengthCounter + _inputArray[_arrayCounter+1].Length;
                    char decidingSymbol = _input[decidingSymbolIndex];
                    decidingPrecedence = GetPrecedence(decidingSymbol);
                }
                else
                {
                    decidingPrecedence = precedence;
                }

                char symbol = _input[_arrayLengthCounter-1];

                _arrayCounter++;
                if (decidingPrecedence <= precedence)
                {
                    sum = ProcessCalculation(symbol, sum, Convert.ToDouble(_inputArray[_arrayCounter]));
                    precedence = decidingPrecedence;
                }
                else
                {
                    sum = ProcessCalculation(symbol,sum ,Calculate(decidingPrecedence));
                }
            }

            return sum;
        }

        private Precedence GetPrecedence(char symbol)
        {
            return symbol == '+' || symbol == '-' ? Precedence.First : Precedence.Second;
        }

        private double ProcessCalculation(char symbol, double number1, double number2)
        {
            switch (symbol)
            {
                case '+': return number1 + number2;
                case '-': return number1 - number2;
                case '*': return number1 * number2;
                case '/': return number1 / number2;
                default: throw new ArgumentException();
            }
        }

        private void Clean() //Refactor to handle more clients, use local methods.
        {
            _inputArray = default;
            _input = default;
            _arrayCounter = default;
            _arrayLengthCounter = default;
        }
    }
}
