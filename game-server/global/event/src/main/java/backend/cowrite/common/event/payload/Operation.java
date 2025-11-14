package backend.cowrite.common.event.payload;

public class Operation {
    OperationType type = OperationType.MOVE;
    Long playerId;
    Double x;
    Double y;
    Double z;
}
