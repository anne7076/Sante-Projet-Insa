using System;
using System.IO.Ports;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace hapticDriver
{
    //Classe de gestion de la communication avec le microcontroleur
    internal class Driver
    {
        /*
         * Attributs posés pour raisons pratiques
        */
        // Marqueur de fin de cahine explicitement nommé
        public static byte EndMarker = 255;

        // Taille du message
        private static readonly int size = 5;

        /*
         * Attributs principaux
         */
        // Port série pour la com
        private readonly SerialPort _serialPort;

        // stockage du message précédent
        private readonly byte[] last_message;
        private byte max = 254;

        // message envoyé au port serie
        private readonly byte[] message;

        // Min et max
        private byte min = 0;

        // Nom du port série
        private readonly string port;

        // Mode verbeux pour le suivi sur la console
        private bool verbose = true;

        // Mode mock pour les tests sans interface
        private bool isMock = false;

        // Initialisation
        public Driver(string new_port, bool isMock = false)
        {
            // Initialisation des attributs
            message = new byte[size];
            message[0] = 0;
            message[1] = 0;
            message[2] = 0;
            message[3] = EndMarker;
            last_message = new byte[size];
            last_message[0] = 0;
            last_message[1] = 0;
            last_message[2] = 0;
            last_message[3] = EndMarker;

            this.isMock = isMock;

            //Initialisation du port serie
            if (!isMock) {
                port = new_port;
                _serialPort = new SerialPort(port, 115200);
                _serialPort.Open();
            }
        }


        // Setter pour verbose
        public bool SetVerbose(bool new_verbose)
        {
            verbose = new_verbose;
            return verbose;
        }

        // Setter pour min et max
        public void SetMinMax(byte new_min, byte new_max)
        {
            min = new_min > 0 && new_min < EndMarker ? new_min : min;
            max = new_max > new_min && new_max < EndMarker ? new_max : max;
        }

        // Getter pour min
        public byte GetMin()
        {
            return min;
        }

        // Getter pour max
        public byte GetMax()
        {
            return max;
        }

        // Getter pour last_message
        public byte[] GetLastMessage()
        {
            return last_message;
        }

        // Setter du message envoyé au port serie
        public void SetMessage(byte[] new_message)
        {
            Array.Copy(message, last_message, size);

            var corrected_new_message = new byte[size];
            Array.Copy(new_message, corrected_new_message, size);

            for (var index = 0; index < size - 1; index++)
            {
                corrected_new_message[index] = corrected_new_message[index] < min ? min : corrected_new_message[index];
                corrected_new_message[index] = corrected_new_message[index] > max ? max : corrected_new_message[index];
            }

            Array.Copy(corrected_new_message, message, size);

            ShowMessage();
        }

        // Envoi du message sur le port serie
        public void SendMessage()
        {
            if (!isMock) _serialPort.Write(message, 0, size);
        }

        // Suivi de message sur la console
        public void ShowMessage()
        {
            if (verbose) {
                string prefix = isMock ? "[Mock]" : "";
                Debug.Log($"{prefix} Message: {message[0]} {message[1]} {message[2]} {message[3]}");
            }
        }

        // Fermeture du port serie
        public void Close()
        {
            if (!isMock) _serialPort.Close();
        }
    }
}