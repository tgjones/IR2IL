#include<stdio.h>
 

static  signed   int  a=-10;
static  signed   char b=-20;

struct tag1  {
  signed    int    *      i01[2];
  signed    char   *      c01[2];
   };

struct tag2  {
  signed    int    *      i02[2];
  struct    tag1          st02[2];
   };

struct tag3  {
         int           i01[2];
         char          c01[2];
   };

struct tag4  {
         int           i02[2];
         struct tag3   st02[2];
   };
int main()
{

 struct tag2 st31[2]= {
   &a,&a,&a,&a,&b,&b,&a,&a,&b,&b,&a,&a,&a,&a,&b,&b,&a,&a,&b,&b };

 struct tag2 st32[2]= {
 {{&a,&a},{{{&a,&a},{&b,&b}},{{&a,&a},{&b,&b}}}},
 {&a,&a,&a,&a,&b,&b,&a,&a,&b,&b}};

 struct tag2 st33[ ]= {  
   &a,&a,&a,&a,&b,&b,&a,&a,&b,&b,&a,&a,&a,&a,&b,&b,&a,&a,&b,&b };

 struct tag2 st34[ ]= { 
 {{&a,&a},{{{&a,&a},{&b,&b}},{{&a,&a},{&b,&b}}}},
 {&a,&a,&a,&a,&b,&b,&a,&a,&b,&b}};

 struct tag4 volatile const  st35[2]= {
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};

 struct tag4 volatile const  st36[2]= {
 {{1,2},{{{3,4},{5,6}},{{7,8},{9,0}}}},
 {1,2,3,4,5,6,7,8,9,0}};

 struct tag4 volatile const  st37[ ]= { 
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};

 struct tag4 volatile const st38[ ]= { 
 {{1,2},{{{3,4},{5,6}},{{7,8},{9,0}}}},
 {1,2,3,4,5,6,7,8,9,0}};

 struct tag4 st39[2]= {
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};

 struct tag4 st40[2]= {
 {{1,2},{{{3,4},{5,6}},{{7,8},{9,0}}}},
 {1,2,3,4,5,6,7,8,9,0}};

 struct tag4 st41[ ]= { 
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0};

 struct tag4 st42[ ]= { 
 {{1,2},{{{3,4},{5,6}},{{7,8},{9,0}}}},
 {1,2,3,4,5,6,7,8,9,0}};

 struct tag2 st43[1][1][1][1][2]= {
   &a,&a,&a,&a,&b,&b,&a,&a,&b,&b,&a,&a,&a,&a,&b,&b,&a,&a,&b,&b };

 struct tag2 st44[1][1][1][1][2]= {
 {{{{{ {{&a}},
 { {{{&a}},{{&b}}},{{{&a}},{{&b}}} }},
 {&a,&a,&a,&a,&b,{&b},{&a} }}}}}
 };

 struct tag2 st45[1][1][1][1][2]= {
 {{{{{ {&a},
 { {{&a},{&b}},{{&a},{&b}} }},
 {&a,&a,&a,&a,&b,{&b},{&a} }}}}}
 };

 struct tag2 st46[ ][1][1][1][2]= {  
   &a,&a,&a,&a,&b,&b,&a,&a,&b,&b,&a,&a,&a,&a,&b,&b,&a,&a,&b,&b };

 struct tag2 st47[ ][1][1][1][2]= { 
 {{{{{ {{&a}},
 { {{{&a}},{{&b}}},{{{&a}},{{&b}}} }},
 {&a,&a,&a,&a,&b,{&b},{&a} }}}}}
 };

 struct tag2 st48[ ][1][1][1][2]= { 
 {{{{{ {&a},
 { {{&a},{&b}},{{&a},{&b}} }},
 {&a,&a,&a,&a,&b,{&b},{&a} }}}}}
 };

 struct tag4 volatile const st49[1][1][1][1][2]= {
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0 };

 struct tag4 volatile const st50[1][1][1][1][2]= {
 {{{{{ {{ 1}},
 { {{{ 3}},{{ 5}}},{{{ 7}},{{ 9}}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

 struct tag4 volatile const st51[1][1][1][1][2]= {
 {{{{{ { 1},
 { {{ 3},{ 5}},{{ 7},{ 9}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

 struct tag4 volatile const st52[ ][1][1][1][2]= { 
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0 };

 struct tag4 volatile const st53[ ][1][1][1][2]= { 
 {{{{{ {{ 1}},
 { {{{ 3}},{{ 5}}},{{{ 7}},{{ 9}}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

 struct tag4 volatile const st54[ ][1][1][1][2]= { 
 {{{{{ { 1},
 { {{ 3},{ 5}},{{ 7},{ 9}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

 struct tag4 st55[1][1][1][1][2]= {
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0 };

 struct tag4 st56[1][1][1][1][2]= {
 {{{{{ {{ 1}},
 { {{{ 3}},{{ 5}}},{{{ 7}},{{ 9}}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

 struct tag4 st57[1][1][1][1][2]= {
 {{{{{ { 1},
 { {{ 3},{ 5}},{{ 7},{ 9}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

 struct tag4 st58[ ][1][1][1][2]= { 
   1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0 };

 struct tag4 st59[ ][1][1][1][2]= { 
 {{{{{ {{ 1}},
 { {{{ 3}},{{ 5}}},{{{ 7}},{{ 9}}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

 struct tag4 st60[ ][1][1][1][2]= { 
 {{{{{ { 1},
 { {{ 3},{ 5}},{{ 7},{ 9}} }},
 { 1, 2, 3, 4, 5,{ 6},{ 7} }}}}}
 };

   printf("*** 02 ** MK3 TEST ** STARTED ***\n");

   if((*(st31[0].i02[0])== -10) &&
      (*(st31[0].i02[1])== -10) &&
      (*(st31[0].st02[0].i01[0])== -10) &&
      (*(st31[0].st02[0].i01[1])== -10) &&
      (*(st31[0].st02[0].c01[0])== -20) &&
      (*(st31[0].st02[0].c01[1])== -20) &&
      (*(st31[0].st02[1].i01[0])== -10) &&
      (*(st31[0].st02[1].i01[1])== -10) &&
      (*(st31[0].st02[1].c01[0])== -20) &&
      (*(st31[0].st02[1].c01[1])== -20))
   printf("*** 02 - 01 ** OK ***\n");
   else
   printf("*** 02 - 01 ** NG ****\n");

   if((*(st32[0].i02[0])== -10) &&
      (*(st32[0].i02[1])== -10) &&
      (*(st32[0].st02[0].i01[0])== -10) &&
      (*(st32[0].st02[0].i01[1])== -10) &&
      (*(st32[0].st02[0].c01[0])== -20) &&
      (*(st32[0].st02[0].c01[1])== -20) &&
      (*(st32[0].st02[1].i01[0])== -10) &&
      (*(st32[0].st02[1].i01[1])== -10) &&
      (*(st32[0].st02[1].c01[0])== -20) &&
      (*(st32[0].st02[1].c01[1])== -20))
   printf("*** 02 - 02 ** OK ***\n");
   else
   printf("*** 02 - 02 ** NG ****\n");

   if((*(st33[0].i02[0])== -10) &&
      (*(st33[0].i02[1])== -10) &&
      (*(st33[0].st02[0].i01[0])== -10) &&
      (*(st33[0].st02[0].i01[1])== -10) &&
      (*(st33[0].st02[0].c01[0])== -20) &&
      (*(st33[0].st02[0].c01[1])== -20) &&
      (*(st33[0].st02[1].i01[0])== -10) &&
      (*(st33[0].st02[1].i01[1])== -10) &&
      (*(st33[0].st02[1].c01[0])== -20) &&
      (*(st33[0].st02[1].c01[1])== -20))
   printf("*** 02 - 03 ** OK ***\n");
   else
   printf("*** 02 - 03 ** NG ****\n");

   if((*(st34[0].i02[0])== -10) &&
      (*(st34[0].i02[1])== -10) &&
      (*(st34[0].st02[0].i01[0])== -10) &&
      (*(st34[0].st02[0].i01[1])== -10) &&
      (*(st34[0].st02[0].c01[0])== -20) &&
      (*(st34[0].st02[0].c01[1])== -20) &&
      (*(st34[0].st02[1].i01[0])== -10) &&
      (*(st34[0].st02[1].i01[1])== -10) &&
      (*(st34[0].st02[1].c01[0])== -20) &&
      (*(st34[0].st02[1].c01[1])== -20))
   printf("*** 02 - 04 ** OK ***\n");
   else
   printf("*** 02 - 04 ** NG ****\n");

   if(( (st35[0].i02[0])==  1 ) &&
      ( (st35[0].i02[1])==  2 ) &&
      ( (st35[0].st02[0].i01[0])==  3 ) &&
      ( (st35[0].st02[0].i01[1])==  4 ) &&
      ( (st35[0].st02[0].c01[0])==  5 ) &&
      ( (st35[0].st02[0].c01[1])==  6 ) &&
      ( (st35[0].st02[1].i01[0])==  7 ) &&
      ( (st35[0].st02[1].i01[1])==  8 ) &&
      ( (st35[0].st02[1].c01[0])==  9 ) &&
      ( (st35[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 05 ** OK ***\n");
   else
   printf("*** 02 - 05 ** NG ****\n");

   if(( (st36[0].i02[0])==  1 ) &&
      ( (st36[0].i02[1])==  2 ) &&
      ( (st36[0].st02[0].i01[0])==  3 ) &&
      ( (st36[0].st02[0].i01[1])==  4 ) &&
      ( (st36[0].st02[0].c01[0])==  5 ) &&
      ( (st36[0].st02[0].c01[1])==  6 ) &&
      ( (st36[0].st02[1].i01[0])==  7 ) &&
      ( (st36[0].st02[1].i01[1])==  8 ) &&
      ( (st36[0].st02[1].c01[0])==  9 ) &&
      ( (st36[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 06 ** OK ***\n");
   else
   printf("*** 02 - 06 ** NG ****\n");

   if(( (st37[0].i02[0])==  1 ) &&
      ( (st37[0].i02[1])==  2 ) &&
      ( (st37[0].st02[0].i01[0])==  3 ) &&
      ( (st37[0].st02[0].i01[1])==  4 ) &&
      ( (st37[0].st02[0].c01[0])==  5 ) &&
      ( (st37[0].st02[0].c01[1])==  6 ) &&
      ( (st37[0].st02[1].i01[0])==  7 ) &&
      ( (st37[0].st02[1].i01[1])==  8 ) &&
      ( (st37[0].st02[1].c01[0])==  9 ) &&
      ( (st37[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 07 ** OK ***\n");
   else
   printf("*** 02 - 07 ** NG ****\n");

   if(( (st38[0].i02[0])==  1 ) &&
      ( (st38[0].i02[1])==  2 ) &&
      ( (st38[0].st02[0].i01[0])==  3 ) &&
      ( (st38[0].st02[0].i01[1])==  4 ) &&
      ( (st38[0].st02[0].c01[0])==  5 ) &&
      ( (st38[0].st02[0].c01[1])==  6 ) &&
      ( (st38[0].st02[1].i01[0])==  7 ) &&
      ( (st38[0].st02[1].i01[1])==  8 ) &&
      ( (st38[0].st02[1].c01[0])==  9 ) &&
      ( (st38[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 08 ** OK ***\n");
   else
   printf("*** 02 - 08 ** NG ****\n");

   if(( (st39[0].i02[0])==  1 ) &&
      ( (st39[0].i02[1])==  2 ) &&
      ( (st39[0].st02[0].i01[0])==  3 ) &&
      ( (st39[0].st02[0].i01[1])==  4 ) &&
      ( (st39[0].st02[0].c01[0])==  5 ) &&
      ( (st39[0].st02[0].c01[1])==  6 ) &&
      ( (st39[0].st02[1].i01[0])==  7 ) &&
      ( (st39[0].st02[1].i01[1])==  8 ) &&
      ( (st39[0].st02[1].c01[0])==  9 ) &&
      ( (st39[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 09 ** OK ***\n");
   else
   printf("*** 02 - 09 ** NG ****\n");

   if(( (st40[0].i02[0])==  1 ) &&
      ( (st40[0].i02[1])==  2 ) &&
      ( (st40[0].st02[0].i01[0])==  3 ) &&
      ( (st40[0].st02[0].i01[1])==  4 ) &&
      ( (st40[0].st02[0].c01[0])==  5 ) &&
      ( (st40[0].st02[0].c01[1])==  6 ) &&
      ( (st40[0].st02[1].i01[0])==  7 ) &&
      ( (st40[0].st02[1].i01[1])==  8 ) &&
      ( (st40[0].st02[1].c01[0])==  9 ) &&
      ( (st40[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 10 ** OK ***\n");
   else
   printf("*** 02 - 10 ** NG ****\n");

   if(( (st41[0].i02[0])==  1 ) &&
      ( (st41[0].i02[1])==  2 ) &&
      ( (st41[0].st02[0].i01[0])==  3 ) &&
      ( (st41[0].st02[0].i01[1])==  4 ) &&
      ( (st41[0].st02[0].c01[0])==  5 ) &&
      ( (st41[0].st02[0].c01[1])==  6 ) &&
      ( (st41[0].st02[1].i01[0])==  7 ) &&
      ( (st41[0].st02[1].i01[1])==  8 ) &&
      ( (st41[0].st02[1].c01[0])==  9 ) &&
      ( (st41[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 11 ** OK ***\n");
   else
   printf("*** 02 - 11 ** NG ****\n");

   if(( (st42[0].i02[0])==  1 ) &&
      ( (st42[0].i02[1])==  2 ) &&
      ( (st42[0].st02[0].i01[0])==  3 ) &&
      ( (st42[0].st02[0].i01[1])==  4 ) &&
      ( (st42[0].st02[0].c01[0])==  5 ) &&
      ( (st42[0].st02[0].c01[1])==  6 ) &&
      ( (st42[0].st02[1].i01[0])==  7 ) &&
      ( (st42[0].st02[1].i01[1])==  8 ) &&
      ( (st42[0].st02[1].c01[0])==  9 ) &&
      ( (st42[0].st02[1].c01[1])==  0 ))
   printf("*** 02 - 12 ** OK ***\n");
   else
   printf("*** 02 - 12 ** NG ****\n");

   if((*(st43[0][0][0][0][0].i02[0])== -10) &&
      (*(st43[0][0][0][0][0].i02[1])== -10) &&
      (*(st43[0][0][0][0][0].st02[0].i01[0])== -10) &&
      (*(st43[0][0][0][0][0].st02[0].i01[1])== -10) &&
      (*(st43[0][0][0][0][0].st02[0].c01[0])== -20) &&
      (*(st43[0][0][0][0][0].st02[0].c01[1])== -20) &&
      (*(st43[0][0][0][0][0].st02[1].i01[0])== -10) &&
      (*(st43[0][0][0][0][0].st02[1].i01[1])== -10) &&
      (*(st43[0][0][0][0][0].st02[1].c01[0])== -20) &&
      (*(st43[0][0][0][0][0].st02[1].c01[1])== -20) &&
      (*(st43[0][0][0][0][1].i02[0])== -10) &&
      (*(st43[0][0][0][0][1].i02[1])== -10) &&
      (*(st43[0][0][0][0][1].st02[0].i01[0])== -10) &&
      (*(st43[0][0][0][0][1].st02[0].i01[1])== -10) &&
      (*(st43[0][0][0][0][1].st02[0].c01[0])== -20) &&
      (*(st43[0][0][0][0][1].st02[0].c01[1])== -20) &&
      (*(st43[0][0][0][0][1].st02[1].i01[0])== -10) &&
      (*(st43[0][0][0][0][1].st02[1].i01[1])== -10) &&
      (*(st43[0][0][0][0][1].st02[1].c01[0])== -20) &&
      (*(st43[0][0][0][0][1].st02[1].c01[1])== -20))
   printf("*** 02 - 13 ** OK ***\n");
   else
   printf("*** 02 - 13 ** NG ****\n");

   if((*(st44[0][0][0][0][0].i02[0])== -10) &&
      (*(st44[0][0][0][0][0].st02[0].i01[0])== -10) &&
      (*(st44[0][0][0][0][0].st02[0].c01[0])== -20) &&
      (*(st44[0][0][0][0][0].st02[1].i01[0])== -10) &&
      (*(st44[0][0][0][0][0].st02[1].c01[0])== -20) &&
      (*(st44[0][0][0][0][1].i02[0])== -10) &&
      (*(st44[0][0][0][0][1].i02[1])== -10) &&
      (*(st44[0][0][0][0][1].st02[0].i01[0])== -10) &&
      (*(st44[0][0][0][0][1].st02[0].i01[1])== -10) &&
      (*(st44[0][0][0][0][1].st02[0].c01[0])== -20) &&
      (*(st44[0][0][0][0][1].st02[0].c01[1])== -20) &&
      (*(st44[0][0][0][0][1].st02[1].i01[0])== -10) )
   printf("*** 02 - 14 ** OK ***\n");
   else
   printf("*** 02 - 14 ** NG ****\n");

   if((*(st45[0][0][0][0][0].i02[0])== -10) &&
      (*(st45[0][0][0][0][0].st02[0].i01[0])== -10) &&
      (*(st45[0][0][0][0][0].st02[0].c01[0])== -20) &&
      (*(st45[0][0][0][0][0].st02[1].i01[0])== -10) &&
      (*(st45[0][0][0][0][0].st02[1].c01[0])== -20) &&
      (*(st45[0][0][0][0][1].i02[0])== -10) &&
      (*(st45[0][0][0][0][1].i02[1])== -10) &&
      (*(st45[0][0][0][0][1].st02[0].i01[0])== -10) &&
      (*(st45[0][0][0][0][1].st02[0].i01[1])== -10) &&
      (*(st45[0][0][0][0][1].st02[0].c01[0])== -20) &&
      (*(st45[0][0][0][0][1].st02[0].c01[1])== -20) &&
      (*(st45[0][0][0][0][1].st02[1].i01[0])== -10) )
   printf("*** 02 - 15 ** OK ***\n");
   else
   printf("*** 02 - 15 ** NG ****\n");

   if((*(st46[0][0][0][0][0].i02[0])== -10) &&
      (*(st46[0][0][0][0][0].i02[1])== -10) &&
      (*(st46[0][0][0][0][0].st02[0].i01[0])== -10) &&
      (*(st46[0][0][0][0][0].st02[0].i01[1])== -10) &&
      (*(st46[0][0][0][0][0].st02[0].c01[0])== -20) &&
      (*(st46[0][0][0][0][0].st02[0].c01[1])== -20) &&
      (*(st46[0][0][0][0][0].st02[1].i01[0])== -10) &&
      (*(st46[0][0][0][0][0].st02[1].i01[1])== -10) &&
      (*(st46[0][0][0][0][0].st02[1].c01[0])== -20) &&
      (*(st46[0][0][0][0][0].st02[1].c01[1])== -20) &&
      (*(st46[0][0][0][0][1].i02[0])== -10) &&
      (*(st46[0][0][0][0][1].i02[1])== -10) &&
      (*(st46[0][0][0][0][1].st02[0].i01[0])== -10) &&
      (*(st46[0][0][0][0][1].st02[0].i01[1])== -10) &&
      (*(st46[0][0][0][0][1].st02[0].c01[0])== -20) &&
      (*(st46[0][0][0][0][1].st02[0].c01[1])== -20) &&
      (*(st46[0][0][0][0][1].st02[1].i01[0])== -10) &&
      (*(st46[0][0][0][0][1].st02[1].i01[1])== -10) &&
      (*(st46[0][0][0][0][1].st02[1].c01[0])== -20) &&
      (*(st46[0][0][0][0][1].st02[1].c01[1])== -20))
   printf("*** 02 - 16 ** OK ***\n");
   else
   printf("*** 02 - 16 ** NG ****\n");

   if((*(st47[0][0][0][0][0].i02[0])== -10) &&
      (*(st47[0][0][0][0][0].st02[0].i01[0])== -10) &&
      (*(st47[0][0][0][0][0].st02[0].c01[0])== -20) &&
      (*(st47[0][0][0][0][0].st02[1].i01[0])== -10) &&
      (*(st47[0][0][0][0][0].st02[1].c01[0])== -20) &&
      (*(st47[0][0][0][0][1].i02[0])== -10) &&
      (*(st47[0][0][0][0][1].i02[1])== -10) &&
      (*(st47[0][0][0][0][1].st02[0].i01[0])== -10) &&
      (*(st47[0][0][0][0][1].st02[0].i01[1])== -10) &&
      (*(st47[0][0][0][0][1].st02[0].c01[0])== -20) &&
      (*(st47[0][0][0][0][1].st02[0].c01[1])== -20) &&
      (*(st47[0][0][0][0][1].st02[1].i01[0])== -10) )
   printf("*** 02 - 17 ** OK ***\n");
   else
   printf("*** 02 - 17 ** NG ****\n");

   if((*(st48[0][0][0][0][0].i02[0])== -10) &&
      (*(st48[0][0][0][0][0].st02[0].i01[0])== -10) &&
      (*(st48[0][0][0][0][0].st02[0].c01[0])== -20) &&
      (*(st48[0][0][0][0][0].st02[1].i01[0])== -10) &&
      (*(st48[0][0][0][0][0].st02[1].c01[0])== -20) &&
      (*(st48[0][0][0][0][1].i02[0])== -10) &&
      (*(st48[0][0][0][0][1].i02[1])== -10) &&
      (*(st48[0][0][0][0][1].st02[0].i01[0])== -10) &&
      (*(st48[0][0][0][0][1].st02[0].i01[1])== -10) &&
      (*(st48[0][0][0][0][1].st02[0].c01[0])== -20) &&
      (*(st48[0][0][0][0][1].st02[0].c01[1])== -20) &&
      (*(st48[0][0][0][0][1].st02[1].i01[0])== -10) )
   printf("*** 02 - 18 ** OK ***\n");
   else
   printf("*** 02 - 18 ** NG ****\n");

   if(( (st49[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st49[0][0][0][0][0].i02[1])==  2 ) &&
      ( (st49[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st49[0][0][0][0][0].st02[0].i01[1])==  4 ) &&
      ( (st49[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st49[0][0][0][0][0].st02[0].c01[1])==  6 ) &&
      ( (st49[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st49[0][0][0][0][0].st02[1].i01[1])==  8 ) &&
      ( (st49[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st49[0][0][0][0][0].st02[1].c01[1])==  0 ) &&
      ( (st49[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st49[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st49[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st49[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st49[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st49[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st49[0][0][0][0][1].st02[1].i01[0])==  7 ) &&
      ( (st49[0][0][0][0][1].st02[1].i01[1])==  8 ) &&
      ( (st49[0][0][0][0][1].st02[1].c01[0])==  9 ) &&
      ( (st49[0][0][0][0][1].st02[1].c01[1])==  0 ))
   printf("*** 02 - 19 ** OK ***\n");
   else
   printf("*** 02 - 19 ** NG ****\n");

   if(( (st50[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st50[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st50[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st50[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st50[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st50[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st50[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st50[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st50[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st50[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st50[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st50[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 20 ** OK ***\n");
   else
   printf("*** 02 - 20 ( ng ****'\n");

   if(( (st51[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st51[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st51[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st51[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st51[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st51[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st51[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st51[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st51[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st51[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st51[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st51[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 21 ** OK ***\n");
   else
   printf("*** 02 - 21 ** NG ****\n");

   if(( (st52[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st52[0][0][0][0][0].i02[1])==  2 ) &&
      ( (st52[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st52[0][0][0][0][0].st02[0].i01[1])==  4 ) &&
      ( (st52[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st52[0][0][0][0][0].st02[0].c01[1])==  6 ) &&
      ( (st52[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st52[0][0][0][0][0].st02[1].i01[1])==  8 ) &&
      ( (st52[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st52[0][0][0][0][0].st02[1].c01[1])==  0 ) &&
      ( (st52[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st52[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st52[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st52[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st52[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st52[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st52[0][0][0][0][1].st02[1].i01[0])==  7 ) &&
      ( (st52[0][0][0][0][1].st02[1].i01[1])==  8 ) &&
      ( (st52[0][0][0][0][1].st02[1].c01[0])==  9 ) &&
      ( (st52[0][0][0][0][1].st02[1].c01[1])==  0 ))
   printf("*** 02 - 22 ** OK ***\n");
   else
   printf("*** 02 - 22 ** NG ****\n");

   if(( (st53[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st53[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st53[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st53[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st53[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st53[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st53[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st53[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st53[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st53[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st53[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st53[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 23 ** OK ***\n");
   else
   printf("*** 02 - 23 ( ng ****'\n");

   if(( (st54[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st54[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st54[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st54[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st54[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st54[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st54[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st54[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st54[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st54[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st54[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st54[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 24 ** OK ***\n");
   else
   printf("*** 02 - 24 ** NG ****\n");

   if(( (st55[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st55[0][0][0][0][0].i02[1])==  2 ) &&
      ( (st55[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st55[0][0][0][0][0].st02[0].i01[1])==  4 ) &&
      ( (st55[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st55[0][0][0][0][0].st02[0].c01[1])==  6 ) &&
      ( (st55[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st55[0][0][0][0][0].st02[1].i01[1])==  8 ) &&
      ( (st55[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st55[0][0][0][0][0].st02[1].c01[1])==  0 ) &&
      ( (st55[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st55[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st55[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st55[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st55[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st55[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st55[0][0][0][0][1].st02[1].i01[0])==  7 ) &&
      ( (st55[0][0][0][0][1].st02[1].i01[1])==  8 ) &&
      ( (st55[0][0][0][0][1].st02[1].c01[0])==  9 ) &&
      ( (st55[0][0][0][0][1].st02[1].c01[1])==  0 ))
   printf("*** 02 - 25 ** OK ***\n");
   else
   printf("*** 02 - 25 ** NG ****\n");

   if(( (st56[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st56[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st56[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st56[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st56[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st56[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st56[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st56[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st56[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st56[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st56[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st56[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 26 ** OK ***\n");
   else
   printf("*** 02 - 26 ( ng ****'\n");

   if(( (st57[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st57[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st57[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st57[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st57[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st57[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st57[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st57[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st57[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st57[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st57[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st57[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 27 ** OK ***\n");
   else
   printf("*** 02 - 27 ** NG ****\n");

   if(( (st58[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st58[0][0][0][0][0].i02[1])==  2 ) &&
      ( (st58[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st58[0][0][0][0][0].st02[0].i01[1])==  4 ) &&
      ( (st58[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st58[0][0][0][0][0].st02[0].c01[1])==  6 ) &&
      ( (st58[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st58[0][0][0][0][0].st02[1].i01[1])==  8 ) &&
      ( (st58[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st58[0][0][0][0][0].st02[1].c01[1])==  0 ) &&
      ( (st58[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st58[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st58[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st58[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st58[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st58[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st58[0][0][0][0][1].st02[1].i01[0])==  7 ) &&
      ( (st58[0][0][0][0][1].st02[1].i01[1])==  8 ) &&
      ( (st58[0][0][0][0][1].st02[1].c01[0])==  9 ) &&
      ( (st58[0][0][0][0][1].st02[1].c01[1])==  0 ))
   printf("*** 02 - 28 ** OK ***\n");
   else
   printf("*** 02 - 28 ** NG ****\n");

   if(( (st59[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st59[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st59[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st59[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st59[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st59[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st59[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st59[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st59[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st59[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st59[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st59[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 29 ** OK ***\n");
   else
   printf("*** 02 - 29 ( ng ****'\n");

   if(( (st60[0][0][0][0][0].i02[0])==  1 ) &&
      ( (st60[0][0][0][0][0].st02[0].i01[0])==  3 ) &&
      ( (st60[0][0][0][0][0].st02[0].c01[0])==  5 ) &&
      ( (st60[0][0][0][0][0].st02[1].i01[0])==  7 ) &&
      ( (st60[0][0][0][0][0].st02[1].c01[0])==  9 ) &&
      ( (st60[0][0][0][0][1].i02[0])==  1 ) &&
      ( (st60[0][0][0][0][1].i02[1])==  2 ) &&
      ( (st60[0][0][0][0][1].st02[0].i01[0])==  3 ) &&
      ( (st60[0][0][0][0][1].st02[0].i01[1])==  4 ) &&
      ( (st60[0][0][0][0][1].st02[0].c01[0])==  5 ) &&
      ( (st60[0][0][0][0][1].st02[0].c01[1])==  6 ) &&
      ( (st60[0][0][0][0][1].st02[1].i01[0])==  7 ) )
   printf("*** 02 - 30 ** OK ***\n");
   else
   printf("*** 02 - 30 ** NG ****\n");

   printf("******* 02 TEST ENDED *******\n");
}