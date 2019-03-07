#ifndef BUTTON_MANAGEMENT_H__
#define BUTTON_MANAGEMENT_H__

typedef enum{
  BUTTON_STATUS_NOT_PRESSED,
  BUTTON_STATUS_PRESSED,
  BUTTON_STATUS_RELEASE
}status_pulse_t;

typedef enum{
  PUSH_TYPE_NULL,  
  PUSH_TYPE_SHORT,  //p<1.5
  PUSH_TYPE_MEDIUM, //1.5<=p<15
  PUSH_TYPE_LONG_I, //15<=p<20
  PUSH_TYPE_LONG_II //p>=20
}type_pulse_t;

/**************************************
* Functions
**************************************/
/**************************************************
* Function name	: void button_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/13
* Description		: Button initialization 
* Notes			: restrictions, odd modes
**************************************************/
void  button_management_init(void);

/**************************************************
* Function name	: void button_managment_attended(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Function to indicate button 
*                 interruption was attended
* Notes			: restrictions, odd modes
**************************************************/
void  button_managment_attended(void);

/**************************************************
* Function name	: void button_management_button_pushed(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/13
* Description		: Initialize timer and button structure as
*                 if an interruption happened
* Notes			: restrictions, odd modes
**************************************************/
void  button_management_button_pushed(void);

/**************************************************
* Function name	: void button_management_new_wake_up_length_value(uint32_t new_wake)
*    returns		: ----
*    args			  : uint32_t new wake_up time value
* Created by		: María Viqueira
* Date created	: 2018/07/27
* Description		: updates value of the length pulse for wake up
* Notes			: restrictions, odd modes
**************************************************/
void  button_management_new_wake_up_length_value(uint32_t new_wake);

/**************************************************
* Function name	: void button_management_new_alarm_dfu_length_value(uint32_t new_alarm)
*    returns		: ----
*    args			  : uint32_t new DFU alarm time value
* Created by		: María Viqueira
* Date created	: 2018/07/27
* Description		: updates value of the length pulse for DFU alarm
* Notes			: restrictions, odd modes
**************************************************/
void  button_management_new_alarm_dfu_length_value(uint32_t new_alarm);

/**************************************************
* Function name	: void button_management_new_enter_dfu_length_value(uint32_t new_dfu)
*    returns		: ----
*    args			  : uint32_t new dfu enter time value
* Created by		: María Viqueira
* Date created	: 2018/07/27
* Description		: updates value of the length pulse for bootloader mode
* Notes			: restrictions, odd modes
**************************************************/
void  button_management_new_enter_dfu_length_value(uint32_t new_dfu);

/**************************************************
* Function name	: uint8_t button_management_status(void)

*    returns		: uint8_t: status of button
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Function to indicate if button is 
*                 pressed, relase or without action
* Notes			: restrictions, odd modes
**************************************************/
uint8_t button_management_status(void);

/**************************************************
* Function name	: uint8_t button_management_type_push(void)

*    returns		: uint8_t: type of pulse
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Function to indicate the type of
*                 pulse: short, medium, long or none
* Notes			: restrictions, odd modes
**************************************************/
uint8_t button_management_type_push(void);

#endif //BUTTON_MANAGEMENT_H__

