﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Contracts.Application.Commands;
using NBB.Contracts.Domain.ContractAggregate;
using NBB.Data.Abstractions;
using NBB.Messaging.Abstractions;

namespace NBB.Contracts.Application.CommandHandlers
{
    public class ContractCommandHandlers :
        IRequestHandler<CreateContract>,
        IRequestHandler<AddContractLine>,
        IRequestHandler<ValidateContract>
    {
        private readonly IEventSourcedRepository<Contract> _repository;
        private readonly IMessageBusPublisher _messageBusPublisher;

        public ContractCommandHandlers(IEventSourcedRepository<Contract> repository, IMessageBusPublisher messageBusPublisher)
        {
            this._repository = repository;
            this._messageBusPublisher = messageBusPublisher;
        }

        public async Task Handle(CreateContract command, CancellationToken cancellationToken)
        {
            var contract = new Contract(command.ClientId);
            await _repository.SaveAsync(contract, cancellationToken);
        }

        public async Task Handle(AddContractLine command, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.AddContractLine(command.Product, command.Price, command.Quantity);
            await _repository.SaveAsync(contract, cancellationToken);
        }

        public async Task Handle(ValidateContract command, CancellationToken cancellationToken)
        {
            var contract = await _repository.GetByIdAsync(command.ContractId, cancellationToken);
            contract.Validate();
            await _repository.SaveAsync(contract, cancellationToken);
        }
    }
}