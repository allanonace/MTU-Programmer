#ifndef TIMERS_MANAGEMENT_H__
#define TIMERS_MANAGEMENT_H__

#define APP_TIMER_PRESCALER             0          /**< Value of the RTC1 PRESCALER register. */
#define TIMER_PAIRING                   60000
#define TIMER_SEC                       APP_TIMER_TICKS(1)
#define TIMER_NO_ACTIVITY               (300*1000)


#define TIMERS_MANGEMENT_MIN_THRES_ON_TIMEOUT        (10*1000)
#define TIMERS_MANGEMENT_MAX_THRES_ON_TIMEOUT        (60*60*1000)
#define TIMERS_MANGEMENT_MIN_THRES_PAIRING_TIMEOUT   (6*1000)
#define TIMERS_MANGEMENT_MAX_THRES_PAIRING_TIMEOUT   (60*1000)

/**@brief Function for the Timer initialization.
 *
 * @details Initializes the timer module. This creates and starts application timers.
 */
void timers_management_init(void);

/**************************************************
* Function name	: void timers_management_control_pairing_start(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Starts pairing timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_control_pairing_start(void);

/**************************************************
* Function name	: void timers_management_control_pairing_stop(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Stops pairing timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_control_pairing_stop(void);

/**************************************************
* Function name	: void timers_management_sec_start(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Starts inactivity timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_sec_start(void);

/**************************************************
* Function name	: void timers_management_sec_stop(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Stops inactivity timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_sec_stop(void);


/**************************************************
* Function name	: void timers_management_new_on_timeout(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new inactivity timeout
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the inactivity timeout
* Notes			: restrictions, odd modes
**************************************************/
void timers_management_new_on_timeout(uint32_t new_time);

/**************************************************
* Function name	: uint32_t  timers_management_get_pairing_timeout_value(void)
*    returns		: uint32_t current value for no activity at PAIRING
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the value of inactivity for going
*                 from PAIRING to ON
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  timers_management_get_pairing_timeout_value(void);

/**************************************************
* Function name	: void timers_management_new_pairing_timeout(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new PAIRING timeout
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the PAIRING timeout
* Notes			: restrictions, odd modes
**************************************************/
void timers_management_new_pairing_timeout(uint32_t new_time);

//void timers_management_control_paired_timmer_attended(void);

/**************************************************
* Function name	: bool timers_management_get_pairing_timer_status(void)
*    returns		: bool: true if timeout of paring was
*                       expired, otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: indicates whether paired timeout expired 
* Notes			    : restrictions, odd modes
**************************************************/
bool timers_management_get_pairing_timer_status(void);

/**************************************************
* Function name	: bool  timers_management_get_on_timeout_status(void)
*    returns		: true if ON timeout was expired, otherwise: false
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Indicates if inactivity timeout was expired or not
* Notes			    : restrictions, odd modes
**************************************************/
bool timers_management_get_on_timeout_status(void);

/**************************************************
* Function name	: uint32_t  timers_management_get_on_timeout_value(void)
*    returns		: uint32_t current value for no activity
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the value of inactivity for going
*                 to SLEEP
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  timers_management_get_on_timeout_value(void);


/**************************************************
* Function name	: void timers_management_reset_on_timeout(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Resets the counter of inactivity timeout
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_reset_on_timeout(void);

#endif

