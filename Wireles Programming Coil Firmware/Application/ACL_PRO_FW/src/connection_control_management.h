#ifndef CONNECTION_MANAGEMENT_H__
#define CONNECTION_MANAGEMENT_H__

#include <stdbool.h>

/**************************************************
* Function name	: void connection_control_management_decode_frame_hello(uint8_t *data_rec)
*    returns		: ---
*    arg 1			: pointer to the frame to check
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: checks whether the received frame is correct or not  
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_decode_frame_hello(uint8_t *data_rec);

/**************************************************
* Function name	: void connection_control_management_reset_connection(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: Sets flag to falses
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_reset_connection(void);

/**************************************************
* Function name	: void connection_control_management_timer_stop(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: stops the timer of the connection control  
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_timer_stop(void);


/**************************************************
* Function name	: void connection_control_management_timer_start(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: starts the timer of the connection control  
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_timer_start(void);

/**************************************************
* Function name	: void connection_control_management_create_timer(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: creates the timer of the connection control  
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_create_timer(void);


/**************************************************
* Function name	: void connection_control_management_timer_attended(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: resets the indicator of connection control expired 
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_timer_attended(void);

/**************************************************
* Function name	: void connection_control_management_update_number_paired_times(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/26
* Description		: increments number of paired times
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_update_number_paired_times(void);

/**************************************************
* Function name	: void connection_control_management_init(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/25
* Description		: reads the GPREGRET for knowing the number or times
*                 the device was paired
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_init(void);

/**************************************************
* Function name	: void connection_control_management_dynamic_pass_was_created(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: indicates dynamic pass was created 
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_dynamic_pass_was_created(void);

/**************************************************
* Function name	: bool connection_control_management_get_connection_valid(void)
*    returns		: bool: true if received frame is correct. Otherwise, false
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: indicates the status of the received frame  
* Notes			    : restrictions, odd modes
**************************************************/  
bool connection_control_management_get_connection_valid(void);

/**************************************************
* Function name	: bool connection_control_management_get_timer_expired(void)
*    returns		: bool with the status of timer of control. True if timer is expired
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: returns the status of the timer of the connection control
* Notes			    : restrictions, odd modes
**************************************************/ 
bool connection_control_management_get_timer_expired(void);

/**************************************************
* Function name	: uint8_t connection_control_management_get_number_paired_times(void)
*    returns		: uint8_t number of pairings
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/25
* Description		: returns the number of paired times of the device
* Notes			    : restrictions, odd modes
**************************************************/ 
uint8_t connection_control_management_get_number_paired_times(void);

//void prueba_connection(void);


#endif 


