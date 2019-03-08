using System;
using nexus.protocols.ble;

namespace ble_library
{
    public interface InterfazPadre {  
        void ONE();   
    }  
   
    public interface InterfazHijo: InterfazPadre {  
        void TWO();  
    }  

    public interface InterfazHija: InterfazPadre {  
        void THREE();  
    }  

    public interface InterfazFamilia: InterfazHijo, InterfazPadre, InterfazHija {
        
    }  

    public interface InterfazInvitado 
    {
        void FOUR();
    }

    public interface IBluetoothEnable {
        void Enable();
        void Disable();
    }


    public class BleMainInterface: InterfazFamilia, InterfazInvitado,IBluetoothEnable
    {  
        public void ONE() 
        {  
            Console.WriteLine("This is ONE");  
        }  
        public void TWO() {  
            Console.WriteLine("This is TWO");  
        }  
        public void THREE() {  
            Console.WriteLine("This is THERE");  
        }
        public void FOUR()
        {
            Console.WriteLine("This is FOUR");
        }


        public void Enable(){
    
        }

        public void Disable(){
            
        }
    }  


}
