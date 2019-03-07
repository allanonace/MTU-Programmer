#ifndef WATCHDOG_MANAGEMENT_H_
#define WATCHDOG_MANAGEMENT_H_

/**************************************************
* Function name	: void watchdog_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/08/01
* Description		: Function for starting watchdog
* Notes			: restrictions, odd modes
**************************************************/
void watchdog_management_init(void);
/**************************************************
* Function name	: void watchdog_management_stop(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/08/01
* Description		: Function for stoppping watchdog
* Notes			: restrictions, odd modes
**************************************************/
void watchdog_management_stop(void);

/**************************************************
* Function name	: void watchdog_management_stop(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/08/01
* Description		: Function for stoppping watchdog
* Notes			: restrictions, odd modes
**************************************************/
void watchdog_management_feed(void);

#define WATCHDOG_SECONDS  10

#endif //WATCHDOG_MANAGEMENT_H_
