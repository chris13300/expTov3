Module modFunctions
    Public Declare Function GetTickCount Lib "kernel32" Alias "GetTickCount" () As Long

    Public Function filename(path As String) As String
        Return My.Computer.FileSystem.GetName(path)
    End Function

    Public Function endHour(start As Long, i As Long, max As Long, Optional prevI As Long = 0, Optional shortFormat As Boolean = False) As String
        If shortFormat Then
            Return Format(DateAdd(DateInterval.Second, (max - i) * ((GetTickCount - start) / 1000) / (i - prevI), Now), "dd/MM/yy HH:mm:ss")
        Else
            Return Format(DateAdd(DateInterval.Second, (max - i) * ((GetTickCount - start) / 1000) / (i - prevI), Now), "dddd' 'd' 'MMM' @ 'HH'h'mm'm'ss")
        End If
    End Function

    Public Function hexa(valeur As Integer) As String
        Dim chaine As String

        chaine = Hex(valeur)
        If Len(chaine) = 1 Then
            chaine = "0" & chaine
        End If
        Return chaine

    End Function

End Module
