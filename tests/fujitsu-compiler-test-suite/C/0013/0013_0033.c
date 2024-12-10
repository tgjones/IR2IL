#include <stdio.h>                                              

struct ASHD00JD61HB00 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned char m3;
} ashd00jd61hb00 = { 1, 2 };

struct ASHD00JD61IB03 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned char m3:3;
} ashd00jd61ib03 = { 1, 2 };

struct ASHD00JD61IB05 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned char m3:5;
} ashd00jd61ib05 = { 1, 2 };

struct ASHD00JD61HH00 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned short m3;
} ashd00jd61hh00 = { 1, 2 };

struct ASHD00JD61IH03 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned short m3:3;
} ashd00jd61ih03 = { 1, 2 };

struct ASHD00JD61IH13 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned short m3:13;
} ashd00jd61ih13 = { 1, 2 };

struct ASHD00JD61HW00 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned long m3;
} ashd00jd61hw00 = { 1, 2 };

struct ASHD00JD61IW03 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned long m3:3;
} ashd00jd61iw03 = { 1, 2 };

struct ASHD00JD61IW29 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned long m3:29;
} ashd00jd61iw29 = { 1, 2 };

struct ASHD00JD61HD00 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned long long m3;
} ashd00jd61hd00 = { 1, 2 };

struct ASHD00JD61ID03 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned long long m3:3;
} ashd00jd61id03 = { 1, 2 };

struct ASHD00JD61ID61 {
	unsigned long long m1;
	unsigned long long   :61;
	unsigned long long m3:61;
} ashd00jd61id61 = { 1, 2 };

struct TAGHB00 {
	unsigned char m1;
};

struct ASHD00JD61HS00 {
	unsigned long long m1;
	unsigned long long   :61;
	struct TAGHB00 m3;
} ashd00jd61hs00 = { 1, { 2 } };

int main() {

	printf("%llu\n", ashd00jd61hb00.m1);
	printf("%u\n"  , ashd00jd61hb00.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61ib03.m1);
	printf("%u\n"  , ashd00jd61ib03.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61ib05.m1);
	printf("%u\n"  , ashd00jd61ib05.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61hh00.m1);
	printf("%u\n"  , ashd00jd61hh00.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61ih03.m1);
	printf("%u\n"  , ashd00jd61ih03.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61ih13.m1);
	printf("%u\n"  , ashd00jd61ih13.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61hw00.m1);
	printf("%lu\n" , ashd00jd61hw00.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61iw03.m1);
	printf("%d\n" , ashd00jd61iw03.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61iw29.m1);
	printf("%d\n" , ashd00jd61iw29.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61hd00.m1);
	printf("%llu\n", ashd00jd61hd00.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61id03.m1);
	printf("%llu\n", (unsigned long long)ashd00jd61id03.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61id61.m1);
	printf("%llu\n", (unsigned long long)ashd00jd61id61.m3);
	printf("\n");

	printf("%llu\n", ashd00jd61hs00.m1);
	printf("%u\n"  , ashd00jd61hs00.m3.m1);
	printf("\n");

	return 0;
}