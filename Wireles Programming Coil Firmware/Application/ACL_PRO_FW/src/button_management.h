#ifndef BUTTON_MANAGEMENT_H__
#define BUTTON_MANAGEMENT_H__

#define LENGTH_MDIUM_PULSE  1500 //length ms
#define LENGTH_LONG_PULSE   3000 //length ms

#define BUTTON_MANAGEMENT_MIN_THRES_ON_TO_PAIRING      (3*1000)
#define BUTTON_MANAGEMENT_MAX_THRES_ON_TO_PAIRING      (10*1000)
#define BUTTON_MANAGEMENT_MIN_THRES_POWER_OFF          (1500)
#define BUTTON_MANAGEMENT_MAX_THRES_POWER_OFF          (5*1000)

#define BUTTON_MANAGEMENT_MIN_DISTANCE_BETWEEN_PULSE    (1*1000)

typedef enum{
  BUTTON_STATUS_NOT_PRESSED,
  BUTTON_STATUS_PRESSED,
  BUTTON_STATUS_RELEASE
}status_pulse_t;

typedef enum{
  PUSH_TYPE_NULL,  
  PUSH_TYPE_SHORT,  //p<1.5
  PUSH_TYPE_MEDIUM, //1.5<=p<3
  PUSH_TYPE_LONG    //p>=3
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
void    button_management_init(void);

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
void    button_managment_attended(void);

/**************************************************
* Function name	: void button_management_new_power_off_value(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new power off time value
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the length pulse for power off
* Notes			: restrictions, odd modes
**************************************************/
void    button_management_new_power_off_value(uint32_t new_time);

/**************************************************
* Function name	: void button_management_new_on_to_pairing_value(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new ON to PAIRING time value
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the length pulse for ON to PAIRING
* Notes			: restrictions, odd modes
**************************************************/
void    button_management_new_on_to_pairing_value(uint32_t new_time);

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
uint8_t   button_management_status(void);

/**************************************************
* Function name	: uint8_t button_management_status(void)

*    returns		: uint8_t: type of pulse
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Function to indicate the type of
*                 pulse: short, medium, long or none
* Notes			: restrictions, odd modes
**************************************************/
uint8_t   button_management_type_push(void);

/**************************************************
* Function name	: uint32_t  button_management_get_on_to_pairing_value(void)
*    returns		: uint32_t current long press value
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the length of the pulse for changing from ON 
*                 to PAIRING
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  button_management_get_on_to_pairing_value(void);

/**************************************************
* Function name	: uint32_t  button_management_get_power_off_value(void)
*    returns		: uint32_t current medium press value
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the length of the pulse for going to SLEEP
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  button_management_get_power_off_value(void);


#endif //BUTTON_MANAGEMENT_H__

