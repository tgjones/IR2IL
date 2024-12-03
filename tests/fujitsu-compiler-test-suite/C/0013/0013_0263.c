#include <stdio.h>                                              

struct ASJB00ID61HB00 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned char m3;
} asjb00id61hb00 = { 1, 2 };

struct ASJB00ID61IB03 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned char m3:3;
} asjb00id61ib03 = { 1, 2 };

struct ASJB00ID61IB05 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned char m3:5;
} asjb00id61ib05 = { 1, 2 };

struct ASJB00ID61HH00 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned short m3;
} asjb00id61hh00 = { 1, 2 };

struct ASJB00ID61IH03 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned short m3:3;
} asjb00id61ih03 = { 1, 2 };

struct ASJB00ID61IH13 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned short m3:13;
} asjb00id61ih13 = { 1, 2 };

struct ASJB00ID61HW00 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned long m3;
} asjb00id61hw00 = { 1, 2 };

struct ASJB00ID61IW03 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned long m3:3;
} asjb00id61iw03 = { 1, 2 };

struct ASJB00ID61IW29 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned long m3:29;
} asjb00id61iw29 = { 1, 2 };

struct ASJB00ID61HD00 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned long long m3;
} asjb00id61hd00 = { 1, 2 };

struct ASJB00ID61ID03 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned long long m3:3;
} asjb00id61id03 = { 1, 2 };

struct ASJB00ID61ID61 {
	unsigned char   :0;
	unsigned long long m2:61;
	unsigned long long m3:61;
} asjb00id61id61 = { 1, 2 };

struct TAGHB00 {
	unsigned char m1;
};

struct ASJB00ID61HS00 {
	unsigned char   :0;
	unsigned long long m2:61;
	struct TAGHB00 m3;
} asjb00id61hs00 = { 1, { 2 } };

int main() {

	printf("%llu\n", (unsigned long long)asjb00id61hb00.m2);
	printf("%u\n"  , asjb00id61hb00.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61ib03.m2);
	printf("%u\n"  , asjb00id61ib03.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61ib05.m2);
	printf("%u\n"  , asjb00id61ib05.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61hh00.m2);
	printf("%u\n"  , asjb00id61hh00.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61ih03.m2);
	printf("%u\n"  , asjb00id61ih03.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61ih13.m2);
	printf("%u\n"  , asjb00id61ih13.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61hw00.m2);
	printf("%lu\n" , asjb00id61hw00.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61iw03.m2);
	printf("%d\n" , asjb00id61iw03.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61iw29.m2);
	printf("%d\n" , asjb00id61iw29.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61hd00.m2);
	printf("%llu\n", asjb00id61hd00.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61id03.m2);
	printf("%llu\n", (unsigned long long)asjb00id61id03.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61id61.m2);
	printf("%llu\n", (unsigned long long)asjb00id61id61.m3);
	printf("\n");

	printf("%llu\n", (unsigned long long)asjb00id61hs00.m2);
	printf("%u\n"  , asjb00id61hs00.m3.m1);
	printf("\n");

	return 0;
}
