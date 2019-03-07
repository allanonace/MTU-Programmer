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
*    args			  : -----
* Created by		: Mar�a Viqueira
* Date created	: 2018/05/23
* Description		: function for managing deep sleep 
* Notes			: restrictions, odd modes
**************************************************/
void gpio_go_to_deep_sleep(void);

/**************************************************
* Function name	: void gpio_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: Mar�a Viqueira
* Date created	: 2018/05/23
* Description		: GPIO initialization 
* Notes			: restrictions, odd modes
**************************************************/
void gpio_management_init(void);


#endif //GPIO_MANAGEMENT_H__

