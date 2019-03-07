#ifndef DFU_STATES_MANAGEMENT_H__
#define DFU_STATES_MANAGEMENT_H__


typedef enum
{
    STATE_IDLE,
    STATE_SLEEP_PREPARE,
    STATE_WAIT_RELEASE,
    STATE_BUTTON_RELEASED,
    STATE_BOOT_MODE_INIT,
    STATE_BOOT_MODE,
    SATE_LAUNCH_APP
}dfu_states_t;

/**************************************
* Functions
**************************************/

/**************************************************
* Function name	: void dfu_state_machine_trans(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/25
* Description		: Controls the transactions of the
*                 state machine
* Notes			    : restrictions, odd modes
**************************************************/
void dfu_state_machine_trans(void);

/**************************************************
* Function name	: void dfu_state_machine_exe(void)
*    returns		: bool to prevent main application
*                 from going to sleep in some cases
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/25
* Description		: Controls the actions of the
*                 state machine
* Notes			    : restrictions, odd modes
**************************************************/
bool dfu_state_machine_exe(void);

#endif //DFU_STATES_MANAGEMENT_H__


