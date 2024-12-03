#include <stdlib.h>
#include <math.h>
#include  <stdio.h>

struct t {
           int m[1024];
         } s1,s2,*ps;
struct t f1(
            int        i1,
            struct t   s1,
            int        i2,
            struct t   s2
           );
int main()
{
    int lcount;
    int i2[1024];


    printf("********** SCOHDI1 TEST STARTED **********\n");

    for ( lcount = 0 ; lcount <=1023 ; ++lcount)
        s1.m[lcount] = i2[lcount] = lcount;

    s2 = f1(1,s1,2,s1);

    if (s2.m[1] == 1)
        printf("********** SCOHDI1 - 01 OK **********\n");
    else
        printf("********** SCOHDI1 - 01 NG **********\n");

    ps = &s1;


    s2 = f1(ps->m[1],*ps,ps->m[2],*ps);

    if (s2.m[1] == 1)
        printf("********** SCOHDI1 - 01 OK **********\n");
    else
        printf("********** SCOHDI1 - 01 NG **********\n");


    s2 = f1(i2[1],*ps,i2[2],*ps);

    if (s2.m[1] == 1)
        printf("********** SCOHDI1 - 01 OK **********\n");
    else
        printf("********** SCOHDI1 - 01 NG **********\n");


    printf("********** SCOHDI1 TEST ENDED **********\n");

exit (0);
}
struct t f1( i1,s1,i2,s2)
int                       i1    ;
struct t                  s1    ;
int                       i2    ;
struct t                  s2    ;
{
    return s1;
}
