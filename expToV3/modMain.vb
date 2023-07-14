Module modMain

    Sub Main()
        Dim EXPv20File As String, EXPv30File As String
        Dim myString As String, score As Integer
        Dim myStreamReader As IO.FileStream, tabWrites(149999) As Byte
        Dim buffer As Long, tabBuffer() As Byte, index As Long, indexWrites As Long
        Dim tabEXPv2(23) As Byte, tabEXPv30(15) As Byte, nbPositions As Long, start As Integer
        Dim nbDrawScores As Integer, nbPositiveScores As Integer, nbNegativeScores As Integer, nbPositiveMates As Integer, nbNegativeMates As Integer

        EXPv20File = Replace(Command(), """", "")
        If EXPv20File = "" Then
            End
        End If
        EXPv30File = Replace(EXPv20File, ".exp", ".v30")
        My.Computer.FileSystem.WriteAllText(EXPv30File, "Experience version 3.0", False, New System.Text.UTF8Encoding(False))

        myStreamReader = New IO.FileStream(EXPv20File, IO.FileMode.Open)

        'SugaR Experience version 2
        '0123456789abcdef0123456789
        buffer = 26
        ReDim tabBuffer(buffer - 1)
        myStreamReader.Read(tabBuffer, 0, buffer)
        'on se position après cette phrase

        If System.Text.Encoding.UTF8.GetString(tabBuffer) <> "SugaR Experience version 2" Then
            MsgBox(filename(EXPv20File) & " <> experience format v2 !?", MsgBoxStyle.Exclamation)
            End
        End If

        buffer = 24 * 10000 'forcément un multiple de 24
        nbPositions = 0
        indexWrites = 0
        nbDrawScores = 0
        nbPositiveScores = 0
        nbNegativeScores = 0
        nbPositiveMates = 0
        nbNegativeMates = 0
        start = Environment.TickCount


        Console.Write("Conversion exp v2.0 to v3.0... ")
        While myStreamReader.Position < myStreamReader.Length
            If (myStreamReader.Position + buffer) > myStreamReader.Length Then
                buffer = myStreamReader.Length - myStreamReader.Position
            End If

            Try
tentative:
                ReDim tabBuffer(buffer - 1)
            Catch ex As Exception
                buffer = buffer * 0.9
                GoTo tentative
            End Try

            myStreamReader.Read(tabBuffer, 0, buffer)

            For index = 0 To buffer - 1 Step 24
                Array.Copy(tabBuffer, index, tabEXPv2, 0, 24)

                'position key
                'v2 : inversed bytes order
                'v3 : original bytes order
                For i = 0 To 7
                    'v2 : inversed order
                    'v3 : normal order
                    tabEXPv30(i) = tabEXPv2(7 - i)
                Next

                'v3 : nbMoves
                tabEXPv30(8) = 1

                'move
                'v2 : inversed order, 4 bytes
                'v3 : normal order, 2 bytes
                tabEXPv30(9) = tabEXPv2(9)
                tabEXPv30(10) = tabEXPv2(8)

                'score
                'v2 : inversed order, doubled values, 4 bytes
                'v3 : normal order, original values, 3 bytes
                myString = hexa(tabEXPv2(15)) & hexa(tabEXPv2(14)) & hexa(tabEXPv2(13)) & hexa(tabEXPv2(12))
                If tabEXPv2(15) = 0 And tabEXPv2(14) = 0 Then
                    'from v2
                    score = Convert.ToInt64(myString, 16)
                    If hexa(tabEXPv2(13)) = "7C" Then
                        score = (32001 - score) / 2
                        'to v3
                        score = 120000 + score 'positive mate score > 120 000
                        nbPositiveMates = nbPositiveMates + 1
                    Else
                        score = score / 2.08
                        'to v3
                        'positive normal score >= 0
                        If score = 0 Then
                            nbDrawScores = nbDrawScores + 1
                        Else
                            nbPositiveScores = nbPositiveScores + 1
                        End If
                    End If
                ElseIf tabEXPv2(15) = 255 And tabEXPv2(14) = 255 Then
                    'from v2
                    score = 4294967295 - Convert.ToInt64(myString, 16)
                    If hexa(tabEXPv2(13)) = "83" Then
                        score = (32000 - score) / 2
                        'to v3
                        score = 140000 - score 'negative mate score < 140 000
                        nbNegativeMates = nbNegativeMates + 1
                    Else
                        score = score / 2.08
                        'to v3
                        score = 262144 - score 'negative normal score < 262144
                        nbNegativeScores = nbNegativeScores + 1
                    End If
                End If
                myString = hexa(score)
                myString = StrDup(6 - Len(myString), "0") & myString

                tabEXPv30(11) = Convert.ToInt64(myString.Substring(0, 2), 16)
                tabEXPv30(12) = Convert.ToInt64(myString.Substring(2, 2), 16)
                tabEXPv30(13) = Convert.ToInt64(myString.Substring(4, 2), 16)

                'depth
                'v2 : 4 bytes
                'v3 : 1 byte
                tabEXPv30(14) = tabEXPv2(16)

                'count
                'v2 : 4 bytes
                'v3 : 1 byte
                tabEXPv30(15) = tabEXPv2(20)

                nbPositions = nbPositions + 1

                'writes on exp file
                If indexWrites + 16 <= tabWrites.Length Then
                    Array.Copy(tabEXPv30, 0, tabWrites, indexWrites, 16)
                    indexWrites = indexWrites + 16
                Else
                    My.Computer.FileSystem.WriteAllBytes(EXPv30File, tabWrites, True)
                    Array.Clear(tabWrites, 0, tabWrites.Length)
                    indexWrites = 0
                End If

                'statistics
                If nbPositions Mod 200000 = 0 Then
                    Console.Clear()
                    Console.Title = "Conversion @ " & Format(myStreamReader.Position / myStreamReader.Length, "0%") & " : " & Trim(Format(nbPositions / (Environment.TickCount - start), "# ### ### ##0 pos/ms")) & " (" & Trim(Format(nbPositions, "# ### ### ##0")) & "), " & endHour(start, myStreamReader.Position, myStreamReader.Length, , True)

                    Console.WriteLine("Draw eval scores : " & Trim(Format(nbDrawScores, "000 000 000")))

                    Console.WriteLine()

                    Console.WriteLine("Pos. eval scores : " & Trim(Format(nbPositiveScores, "000 000 000")))
                    Console.WriteLine("Neg. eval scores : " & Trim(Format(nbNegativeScores, "000 000 000")))

                    Console.WriteLine()

                    Console.WriteLine("Pos. mate scores : " & Trim(Format(nbPositiveMates, "000 000 000")))
                    Console.WriteLine("Neg. mate scores : " & Trim(Format(nbNegativeMates, "000 000 000")))
                End If

                'cleaning
                Array.Clear(tabEXPv2, 0, tabEXPv2.Length)
                Array.Clear(tabEXPv30, 0, tabEXPv30.Length)
            Next
        End While
        myStreamReader.Close()

        If indexWrites > 0 Then
            ReDim Preserve tabWrites(indexWrites - 1)
            My.Computer.FileSystem.WriteAllBytes(EXPv30File, tabWrites, True)
        End If

        Console.Clear()
        Console.Title = My.Computer.Name & " @ 100% : " & Trim(Format(nbPositions / (Environment.TickCount - start), "# ### ### ##0 pos/ms")) & " (" & Trim(Format(nbPositions, "# ### ### ##0")) & ")"

        Console.WriteLine("Draw eval scores : " & Trim(Format(nbDrawScores, "000 000 000")))

        Console.WriteLine()

        Console.WriteLine("Pos. eval scores : " & Trim(Format(nbPositiveScores, "000 000 000")))
        Console.WriteLine("Neg. eval scores : " & Trim(Format(nbNegativeScores, "000 000 000")))

        Console.WriteLine()

        Console.WriteLine("Pos. mate scores : " & Trim(Format(nbPositiveMates, "000 000 000")))
        Console.WriteLine("Neg. mate scores : " & Trim(Format(nbNegativeMates, "000 000 000")))

        Console.WriteLine()

        Console.WriteLine(filename(EXPv20File) & " : " & Trim(Format(FileLen(EXPv20File) / 1024 / 1024, "# ##0 MB")) & " (exp v2.0)")
        Console.WriteLine(filename(EXPv30File) & " : " & Trim(Format(FileLen(EXPv30File) / 1024 / 1024, "# ##0 MB")) & " (exp v3.0, " & Format(FileLen(EXPv30File) / FileLen(EXPv20File) - 1, "0%") & ")")

        Console.WriteLine()

        Console.WriteLine("Press ENTER to close the window.")
        Console.ReadLine()
    End Sub

    
End Module
