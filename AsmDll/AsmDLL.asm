.data
r DD 0.3f               ; Waga dla kana³u czerwonego (czerwona sk³adowa ma wp³yw w 30%)
g DD 0.59f              ; Waga dla kana³u zielonego (zielona sk³adowa ma wp³yw w 59%)
b DD 0.11f              ; Waga dla kana³u niebieskiego (niebieska sk³adowa ma wp³yw w 11%)

.code
EdgeDetect proc
; Procedura
; Konwertuje piksele obrazu na odcienie szaroœci z u¿yciem wag kolorów.
; Rejestry wejœciowe:
; - RCX: wskaŸnik na kana³ czerwony
; - RDX: wskaŸnik na kana³ zielony
; - R8:  wskaŸnik na kana³ niebieski

    ; £adowanie danych kana³u czerwonego do xmm0
    VMOVUPS xmm0, xmmword ptr [RCX]           ; Pobierz 8 bajtów danych (piksele) z pamiêci wskazanej przez RCX
    VPMOVZXBD ymm0, xmm0                      ; Rozszerz bajty do s³ów (16-bitów) z wype³nieniem zerami
    VCVTDQ2PS ymm0, ymm0                      ; Konwertuj dane ca³kowite na liczby zmiennoprzecinkowe (float)

    ; £adowanie danych kana³u zielonego do xmm1
    VMOVUPS xmm1, xmmword ptr [RDX]           ; Pobierz 8 bajtów danych (piksele) z pamiêci wskazanej przez RDX
    VPMOVZXBD ymm1, xmm1                      ; Rozszerz bajty do s³ów (16-bitów)
    VCVTDQ2PS ymm1, ymm1                      ; Konwertuj dane ca³kowite na liczby zmiennoprzecinkowe (float)

    ; £adowanie danych kana³u niebieskiego do xmm2
    VMOVUPS xmm2, xmmword ptr [R8]            ; Pobierz 8 bajtów danych (piksele) z pamiêci wskazanej przez R8
    VPMOVZXBD ymm2, xmm2                      ; Rozszerz bajty do s³ów (16-bitów)
    VCVTDQ2PS ymm2, ymm2                      ; Konwertuj dane ca³kowite na liczby zmiennoprzecinkowe (float)

    ; Za³adowanie wag kana³ów kolorów
    VBROADCASTSS ymm3, r                      ; Powiel wagê czerwonego kana³u w ca³ym rejestrze ymm3
    VBROADCASTSS ymm4, g                      ; Powiel wagê zielonego kana³u w ca³ym rejestrze ymm4
    VBROADCASTSS ymm5, b                      ; Powiel wagê niebieskiego kana³u w ca³ym rejestrze ymm5

    ; Mno¿enie danych kana³ów przez odpowiednie wagi
    VMULPS ymm0, ymm0, ymm3                   ; Mno¿enie kana³u czerwonego przez jego wagê
    VMULPS ymm1, ymm1, ymm4                   ; Mno¿enie kana³u zielonego przez jego wagê
    VMULPS ymm2, ymm2, ymm5                   ; Mno¿enie kana³u niebieskiego przez jego wagê

    ; Sumowanie wyników w celu uzyskania wartoœci odcienia szaroœci
    VADDPS ymm0, ymm0, ymm1                   ; Dodanie wartoœci kana³u zielonego do kana³u czerwonego
    VADDPS ymm0, ymm0, ymm2                   ; Dodanie wartoœci kana³u niebieskiego do sumy (pe³ny odcieñ szaroœci)

    ; Konwersja wyników na liczby ca³kowite
    VCVTTPS2DQ ymm0, ymm0                     ; Konwersja z liczb zmiennoprzecinkowych na ca³kowite (zaokr¹glenie w dó³)

    ; Zapis przetworzonych danych do pamiêci
    pextrb byte ptr [R9+0], xmm0, 0          ; Zapisanie wartoœci dla pierwszego piksela (czerwony)
    pextrb byte ptr [R9+1], xmm0, 4          ; Zapisanie wartoœci dla pierwszego piksela (zielony)
    pextrb byte ptr [R9+2], xmm0, 8          ; Zapisanie wartoœci dla pierwszego piksela (niebieski)
    pextrb byte ptr [R9+3], xmm0, 12         ; Zapisanie wartoœci dla drugiego piksela (czerwony)

    ; Przeorganizowanie danych w rejestrze ymm0, by obs³u¿yæ kolejne piksele
    VPERMQ ymm0, ymm0, 11111110b              ; Zamiana dolnej i górnej czêœci rejestru (na poziomie 128 bitów)

    ; Zapis przetworzonych danych dla kolejnych pikseli
    pextrb byte ptr [R9+4], xmm0, 0          ; Zapisanie wartoœci dla trzeciego piksela (czerwony)
    pextrb byte ptr [R9+5], xmm0, 4          ; Zapisanie wartoœci dla trzeciego piksela (zielony)
    pextrb byte ptr [R9+6], xmm0, 8          ; Zapisanie wartoœci dla trzeciego piksela (niebieski)
    pextrb byte ptr [R9+7], xmm0, 12         ; Zapisanie wartoœci dla czwartego piksela (czerwony)

    ; Zakoñczenie procedury
    ret                                       ; Powrót do miejsca wywo³ania

EdgeDetect endp
end
