#include<stdio.h>

 volatile struct sttag1 {
       short  s;
       long   l;
 } st1;
 const union untag1 {
       int    i;
       long   l;
 } un1,un12;
 volatile enum tagen1 {
       ena,  enb,  enc
 } en1;
int main()
{
   const struct sttag1 stx={0,0},*stxp;
   volatile struct sttag2 {
         char  c;
         int   i;
   } st2;
   const enum entag2 {
         enx=-1,  eny,  enz=100
   } en1 = enz;
   static volatile union untag2 {
         signed  s;
         unsigned u;
   } un2[2] = {  -1 ,  0  };


   printf("*** \n");

   if (st1.s!=0 || st1.l!=0)
       printf(" *** \n");
   else
       printf(" *** \n");

   if (un1.i!=0 || un1.l!=0 || un12.i!=0 || un12.l!=0)
       printf(" *** \n");
   else
       printf(" *** \n");

   if (en1!=100 || ena!=0 || enb!=1 || enc!=2)
       printf(" *** \n");
   else
       printf(" *** \n");

   stxp = &stx;
   if (stx.s!=stxp->s || stx.l!=stxp->l)
       printf(" *** \n");
   else
       printf(" *** \n");

   st2.c=0x7f; st2.i=-1;
   if (st2.c!=127 || st2.i!=-1)
       printf(" *** \n");
   else
       printf(" *** \n");

   if (en1!=100 || enx!=-1 || eny!=0 || enz!=100)
       printf(" *** \n");
   else
       printf(" *** \n");

   un2[0].s=-1; un2[1].u=0;
#if INT64
   if (un2[0].s!=-1 || un2[0].u!=18446744073709551615 ||
       un2[1].s!=0  || un2[1].u!=0)
#else
   if (un2[0].s!=-1 || un2[0].u!=4294967295 ||
       un2[1].s!=0  || un2[1].u!=0)
#endif
       printf(" *** \n");
   else
       printf(" *** \n");

   printf("*** \n");
}
