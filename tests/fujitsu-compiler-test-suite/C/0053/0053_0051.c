#include<stdio.h>

#include<stdio.h>

 extern volatile const short int signed a4[2]={7,8};
 volatile const int signed short a5;
int main()
{
   const volatile signed short int *addr11;
   const volatile unsigned short int *addr21;

   printf("***  ** MK3 TEST FOR DCL ** STARTED ***\n");

   {
     auto volatile const signed short int a1;
     register volatile const signed int short a2=0,b2=-1;
     static volatile const short unsigned int a3[2];
     extern volatile const short int signed a4[2];
     extern volatile const int signed short a5;
     typedef volatile const int short unsigned type1;
     type1  a6;

     addr11 = &a1;
     if (a1!=*addr11)
         printf(" ***  TEST-01 ERROR ***\n");
     else
         printf(" ***  TEST-01 OK ***\n");

     if (a2!=0 || b2!=-1)
         printf(" ***  TEST-02 ERROR ***\n");
     else
         printf(" ***  TEST-02 OK ***\n");

     addr21 = a3;
     if (a3[0]!=*addr21)
         printf(" ***  TEST-03 ERROR ***\n");
     else
         printf(" ***  TEST-03 OK ***\n");

     if (a4[0]!=7 || a4[1]!=8)
         printf(" ***  TEST-04 ERROR ***\n");
     else
         printf(" ***  TEST-04 OK ***\n");

     if (a5!=0)
         printf(" ***  TEST-05 ERROR ***\n");
     else
         printf(" ***  TEST-05 OK ***\n");

     addr21 = &a6;
     if (a6!=*addr21)
         printf(" ***  TEST-06 ERROR ***\n");
     else
         printf(" ***  TEST-06 OK ***\n");
   }

   printf("***  TEST ENDED ***\n");
}
