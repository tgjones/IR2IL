#include <stdio.h>
int main()
{
    double                 dbl   = 2048;

    char                   ca    = 1;
    int                    ia    = 2;
    short int              sia   = 3;
    long int               lia   = 4;
    long long int          llia  = 5;
    float                  flta  = 0.5;
    double                 dbla  = 0.25;
    long double            ldbla = 0.125;
    signed long int        slia  = -6;
    unsigned char          uca   = 7;

    char                   cb    = 8;
    int                    ib    = 9;
    short int              sib   = 10;
    long int               lib   = 11;
    long long int          llib  = 12;
    float                  fltb  = 0.0625;
    double                 dblb  = 0.03125;
    long double            ldblb = 0.015625;
    signed char            scb   = -13;
    unsigned int           uib   = 14;


    printf("*** TEST FOR EXPRESSION *** STARTED\n");

    if(dbl*ca != 2048)
        printf("***-01*** N   G ***\n");

    if(dbl*ia != 4096)
        printf("***-02*** N   G ***\n");

    if(dbl*sia != 6144)
        printf("***-03*** N   G ***\n");

    if(dbl*lia != 8192)
        printf("***-04*** N   G ***\n");

    if(dbl*llia != 10240)
        printf("***-05*** N   G ***\n");


    if(dbl*slia != -12288)
        printf("***-09*** N   G ***\n");

    if(dbl*uca != 14336)
        printf("***-10*** N   G ***\n");

    if(dbl*2048 != 4194304)
        printf("***-11*** N   G ***\n");

    if(dbl*(int)ca != 2048)
        printf("***-12*** N   G ***\n");

    if(dbl/cb != 256)
        printf("***-13*** N   G ***\n");

    if((int)(dbl/ib) != 227)
        printf("***-14*** N   G ***\n");

    if((int)(dbl/sib) != 204)
        printf("***-15*** N   G ***\n");

    if((int)(dbl/lib) != 186)
        printf("***-16*** N   G ***\n");

    if((int)(dbl/llib) != 170)
        printf("***-17*** N   G ***\n");


    if((int)(dbl/scb) != -157)
        printf("***-21*** N   G ***\n");

    if((int)(dbl/uib) != 146)
        printf("***-22*** N   G ***\n");

    if((int)(dbl/13) != 157)
        printf("***-23*** N   G ***\n");

    if((int)(dbl/(long long int)cb) != 256)
        printf("***-24*** N   G ***\n");


    if(dbl*flta != 1024)
        printf("***-06*** N   G ***\n");

    if(dbl*dbla != 512)
        printf("***-07*** N   G ***\n");

    if(dbl*ldbla != 256)
        printf("***-08*** N   G ***\n");

    if(dbl/fltb != 32768)
        printf("***-18*** N   G ***\n");

    if(dbl/dblb != 65536)
        printf("***-19*** N   G ***\n");

    if(dbl/ldblb != 131072)
        printf("***-20*** N   G ***\n");


    printf("*** TEST FOR EXPRESSION *** ENDED\n");
}
