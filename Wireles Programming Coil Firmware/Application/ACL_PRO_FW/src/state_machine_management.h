#ifndef STATE_MACHINE_MANAGEMENT_H__
#define STATE_MACHINE_MANAGEMENT_H__

#include <stdbool.h>

typedef enum{
  
  STATE_INIT_WAIT_CONNECTION,
  STATE_INIT_CONNECTED,
  STATE_INIT_WAIT_PRESENTATION,
  STATE_INIT_PRESENTATION_OK,
  STATE_INIT_CONTROL_FINISHED,
  STATE_ON,
  STATE_ON_TO_PAIRING,
  STATE_ON_TO_PAIRING_DISCONNECT,
  STATE_ON_TO_PAIRING_WAT_DISCONNECT,
  STATE_PAIRING,
  STATE_PAIRING_TO_ON,
  STATE_PAIRED_SUCCESS,
  STATE_BLE_MASTER_DISCONNECTED,
  STATE_BLE_PERIPHERAL_DISCONNECTED,
  STATE_BLE_WAIT_DISCONNECTION,
  STATE_BLE_DISCONNECTION_SUCCESS,  
  STATE_RESET_DISCONNECT,
  STATE_RESET_WAIT_DISCONNECTION,
  STATE_RESET,
  STATE_SLEEP_PREPARE,
  
  

}general_states_t;


/**************************************
* Functions
**************************************/
/**************************************************
* Function name	: void state_machine_trans(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: Controls the transactions of the
*                 state machine
* Notes			    : restrictions, odd modes
**************************************************/
void state_machine_trans(void);

/**************************************************
* Function name	: void state_machine_trans(void)
*    returns		: bool to prevent main application
*                 from going to sleep in some cases
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: Controls the actions of the
*                 state machine
* Notes			    : restrictions, odd modes
**************************************************/
bool state_machine_exe(void);

#endif //STATE_MACHINE_MANAGEMENT_H__



