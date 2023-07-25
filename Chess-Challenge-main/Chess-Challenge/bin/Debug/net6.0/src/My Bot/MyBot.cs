using ChessChallenge.API;
using System;

//IMPLEMENTAR ESCAPE PARA CUANDO ME AMENAZAN UNA PIEZA

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();

        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];

        int highestValueCapture = 0;
        int highestValueThreat = 0;

        bool checkmate = false;
        bool check = false;
        bool highValueCapture = false;
        bool pieceinthreat = false;
        bool promotion = false;
        bool scape = false;

        foreach (Move move in allMoves)
        {
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                moveToPlay = move;
                checkmate = true;
                break;
            }
        }

        if (!checkmate)
        {
            Move[] allMovesCap = board.GetLegalMoves(true);

            foreach (Move move in allMovesCap)
            {
                Piece myPiece = board.GetPiece(move.StartSquare);
                int myPieceValue = pieceValues[(int)myPiece.PieceType];

                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue >= highestValueCapture)
                {
                    moveToPlay = move;
                    highestValueCapture = capturedPieceValue;
                    highValueCapture = true;
                }
            }
        }

        if (!checkmate && !highValueCapture)
        {
            foreach (Move move in allMoves)
            {
                if (MoveIsCheck(board, move))
                {
                    moveToPlay = move;
                    check = true;
                    break;
                }
            }
        }

        if (!checkmate && !check && !highValueCapture)
        {
            foreach (Move move in allMoves)
            {
                int value = ValueThreat(board, move);

                if (value >= highestValueThreat)
                {
                    moveToPlay = move;
                    pieceinthreat = true;
                    highestValueThreat = value;
                }
            }
        }

        if (!checkmate && !check && !highValueCapture && !pieceinthreat)
        {
            foreach (Move move in allMoves)
            {
                if (move.IsPromotion)
                {
                    moveToPlay = move;
                    promotion = true;
                    break;
                }
            }
        }


        if (!checkmate && !check && !highValueCapture && !pieceinthreat && !promotion)
        {
            highestValueThreat = 0;

            Square sq = FindHighestValueThreat(board);

            foreach (Move move in allMoves)
            {
                int value = ValueThreat(board, move);

                if(move.StartSquare == sq && !board.SquareIsAttackedByOpponent(move.TargetSquare) && value >= highestValueThreat)
                {
                    moveToPlay = move;
                    highestValueThreat = value;
                    scape = true;
                }
            }
        }


        if (!checkmate && !check && !highValueCapture && !pieceinthreat && !promotion && !scape)
        {
            foreach (Move move in allMoves)
            {
                if (move.IsCastles)
                {
                    moveToPlay = move;
                    break;
                }
            }
        }
        else
        {
            while(board.SquareIsAttackedByOpponent(moveToPlay.TargetSquare))
            {
                rng = new();
                moveToPlay = allMoves[rng.Next(allMoves.Length)];
            }
        }

        return moveToPlay;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    bool MoveIsCheck(Board board, Move move)
    {
        board.MakeMove(move);
        bool isCheck = false;

        if (board.IsInCheck() && !board.SquareIsAttackedByOpponent(move.TargetSquare))
            isCheck = true;

        board.UndoMove(move);

        return isCheck;
    }

    int ValueThreat(Board board, Move move)
    {
        int threatValue = 0;

        PieceType myPieceType = move.MovePieceType;

        board.MakeMove(move);

        Move[] allMoves = board.GetLegalMoves(true); // Only captures

        foreach (Move newMove in allMoves)
        {
            int value = pieceValues[(int)newMove.CapturePieceType] - pieceValues[(int)myPieceType];

            if (newMove.MovePieceType == myPieceType && value >= threatValue)
                threatValue = value;
        }

        board.UndoMove(move);

        return threatValue;
    }

    Square FindHighestValueThreat(Board board)
    {
        Move[] allMoves = board.GetLegalMoves(); // Only captures

        int highestVal = 0;
        Square sq = allMoves[0].StartSquare;

        foreach (Move move in allMoves)
        {
            if(board.SquareIsAttackedByOpponent(move.StartSquare) && pieceValues[(int)move.MovePieceType] > highestVal)
            {
                highestVal = pieceValues[(int)move.MovePieceType];
                sq = move.StartSquare;
            }
        }

        return sq;
    }
}