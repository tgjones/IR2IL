#include<stdio.h>

#include<stdio.h>
 volatile const signed a4;
 volatile const signed a5;
 typedef const volatile signed type1;
int main()
{

   volatile const signed *addr,*addr2;

   auto volatile const signed a1=0xffff,b1;
   register volatile const signed a2=-1;
   static volatile const signed a3[2]={2,3};
   extern volatile const signed a4;
   extern volatile const signed a5;
   type1 a6;


   printf("***  ** MK3 TEST FOR DCL ** STARTED ***\n");

   addr = &b1;
   if (a1!=65535 || b1!=*addr)
       printf(" ***  TEST-01 ERROR ***\n");
   else
       printf(" ***  TEST-01 OK ***\n");

   if (a2!=-1)
       printf(" ***  TEST-02 ERROR ***\n");
   else
       printf(" ***  TEST-02 OK ***\n");

   if (a3[0]!=2 || a3[1]!=3)
       printf(" ***  TEST-03 ERROR ***\n");
   else
       printf(" ***  TEST-03 OK ***\n");

   if (a4!=0)
       printf(" ***  TEST-04 ERROR ***\n");
   else
       printf(" ***  TEST-04 OK ***\n");

   if (a5!=0)
       printf(" ***  TEST-05 ERROR ***\n");
   else
       printf(" ***  TEST-05 OK ***\n");

   addr = &a6;
   if (a6!=*addr)
       printf(" ***  TEST-06 ERROR ***\n");
   else
       printf(" ***  TEST-06 OK ***\n");

   printf("***  TEST ENDED ***\n");
}