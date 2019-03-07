#ifndef ENCRYPTION_MANAGEMENT_H__
#define ENCRYPTION_MANAGEMENT_H__

#define AES_BITS    256
#define AES_BYTES   (AES_BITS/8)
#define LEN_MD5     16

/**************************************************
* Function name	: void encryption_management_encrypt_aes(unsigned char *frame_in, 
                            unsigned char *frame_out)
*    returns		: ---
*    arg 1			: pointer to the original frame to be encrypted
*    arg 2      : pointer to the encrypted frame
* Created by		: María Viqueira
* Date created	: 2018/05/25
* Description		: encrypts, using AES, the input frame and copies the result
*                 to output frame  
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_management_encrypt_aes(unsigned char *frame_in, unsigned char *frame_out,uint8_t block_size);

/**************************************************
* Function name	: void encryption_management_decrypt_aes(unsigned char *frame_in, 
                            unsigned char *frame_out)
*    returns		: ---
*    arg 1			: pointer to the encrypted frame
*    arg 2      : pointer to the decrypted frame
* Created by		: María Viqueira
* Date created	: 2018/05/25
* Description		: decrypts, using AES, the input frame and copies the result
*                 to output frame  
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_management_decrypt_aes(unsigned char *frame_in, unsigned char *frame_out,uint8_t block_size);

/**************************************************
* Function name	: void encryption_control_management_create_new_dynamic_pass(void)
*    returns		: ---
*    arg 1			: bool true if we need new seed, otherwise, false
* Created by		: María Viqueira
* Date created	: 2018/07/17. Modified 26/07/2018
* Description		: generates a new dynamic password using MD5
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_control_management_create_new_dynamic_pass(bool new_seed);

/**************************************************
* Function name	: void encryption_control_management_get_dynamic_pass(unsigned char *my_pass)
*    returns		: ---
*    arg 1			: unsigned char* pointer to array to store new pass
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: Returns the dynamic password
* Notes			    : restrictions, odd modes
**************************************************/  
void encryption_control_management_get_dynamic_pass(unsigned char *my_pass );

//void prueba_aes(void);


#endif


