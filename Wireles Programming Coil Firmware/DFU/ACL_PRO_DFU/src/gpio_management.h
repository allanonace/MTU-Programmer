#ifndef GPIO_MANAGEMENT_H__
#define GPIO_MANAGEMENT_H__

typedef enum{
  GPIO_DISCONNECTED,
  GPIO_CONNECTED
}gpio_status_t;

/**************************************
* Functions
**************************************/
/**************************************************
* Function name	: void gpio_go_to_deep_sleep(void)

*    returns		: ----
*    args			  : bool: indicates whether softdevice
*                       must be initialized or not
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: function for managing deep sleep 
* Notes			: restrictions, odd modes
**************************************************/
void gpio_go_to_deep_sleep(bool init_softdevice);

/**************************************************
* Function name	: void gpio_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: GPIO initialization 
* Notes			: restrictions, odd modes
**************************************************/
void gpio_management_init(void);


#endif //GPIO_MANAGEMENT_H__

